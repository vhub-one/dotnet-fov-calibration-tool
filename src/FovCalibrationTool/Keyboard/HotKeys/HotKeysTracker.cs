using Gma.System.MouseKeyHook;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Windows.Forms;

namespace FovCalibrationTool.Keyboard.HotKeys
{
    public class HotKeysTracker
    {
        public async IAsyncEnumerable<HotKeyStatus> TrackAsync(IEnumerable<HotKey> hotKeysList, [EnumeratorCancellation] CancellationToken token)
        {
            var hotKeys = hotKeysList.GroupBy(hk => hk.Keys).ToDictionary(g => g.Key, g => g.ToList());
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
                // Debug.WriteLine($"{hotKey.Keys} [{direction}] + {hotKey.KeysModifier}");

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

            while (true)
            {
                yield return await hotKeysChannel.Reader.ReadAsync(token);
            }
        }
    }
}
