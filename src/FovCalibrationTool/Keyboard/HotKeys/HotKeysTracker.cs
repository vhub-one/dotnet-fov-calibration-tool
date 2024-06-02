using Gma.System.MouseKeyHook;
using System.Threading.Channels;
using System.Windows.Forms;

namespace FovCalibrationTool.Keyboard.HotKeys
{
    public class HotKeysTracker
    {
        public IAsyncEnumerable<Sequence> TrackAsync(IEnumerable<Sequence> sequences, CancellationToken token)
        {
            var sequencesMapChannel = Channel.CreateUnbounded<Sequence>();
            var sequencesMap = new List<KeyValuePair<Sequence, Action>>();

            foreach (var sequence in sequences)
            {
                var sequencePair = KeyValuePair.Create(
                    sequence,
                    () => {
                        sequencesMapChannel.Writer.TryWrite(sequence);
                    }
                );

                sequencesMap.Add(sequencePair);
            }

            ThreadPool.QueueUserWorkItem((state) =>
            {
                using var keyboardEvents = Hook.GlobalEvents();

                keyboardEvents.OnSequence(sequencesMap);

                using var appContext = new ApplicationContext();
                using var appContextTermination = token.Register(() => appContext.ExitThread());

                Application.Run(appContext);
            });

            return sequencesMapChannel.Reader.ReadAllAsync(token);
        }

        public IAsyncEnumerable<HotKeyStatus> TrackAsync(IEnumerable<HotKey> hotKeysList, CancellationToken token)
        {
            var hotKeys = hotKeysList.GroupBy(hk => hk.Keys).ToDictionary(
                g => g.Key,
                g => g.ToList()
            );

            var hotKeysChannel = Channel.CreateUnbounded<HotKeyStatus>();

            void hotKeyDownHandler(object sender, KeyEventArgs e)
            {
                hotKeyHandler(new HotKey(e.KeyCode, e.Modifiers), HotKeyDirection.Down);
            }

            void hotKeyUpHandler(object sender, KeyEventArgs e)
            {
                hotKeyHandler(new HotKey(e.KeyCode, e.Modifiers), HotKeyDirection.Up);
            }

            void hotKeyHandler(HotKey hotKey, HotKeyDirection direction)
            {
                System.Diagnostics.Debug.WriteLine($"{hotKey.Keys} [{direction}] + {hotKey.KeysModifier}");

                var hotKeysRegistered = hotKeys.GetValueOrDefault(hotKey.Keys);

                if (hotKeysRegistered == null)
                {
                    return;
                }

                var hotKeyFound = hotKeysRegistered.Any(hotKey.HasHotKey);

                if (hotKeyFound)
                {
                    var hotKeyStatus = new HotKeyStatus(
                        hotKey,
                        direction
                    );

                    hotKeysChannel.Writer.TryWrite(hotKeyStatus);
                }
            }

            ThreadPool.QueueUserWorkItem((state) =>
            {
                using var keyboardEvents = Hook.GlobalEvents();

                keyboardEvents.KeyDown += hotKeyDownHandler;
                keyboardEvents.KeyUp += hotKeyUpHandler;

                using var appContext = new ApplicationContext();
                using var appContextTermination = token.Register(() => appContext.ExitThread());

                try
                {
                    Application.Run(appContext);
                }
                finally
                {
                    keyboardEvents.KeyDown -= hotKeyDownHandler;
                    keyboardEvents.KeyUp -= hotKeyUpHandler;
                }
            });

            return hotKeysChannel.Reader.ReadAllAsync(token);
        }
    }
}
