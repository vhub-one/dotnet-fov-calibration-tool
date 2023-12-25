using Gma.System.MouseKeyHook;
using Gma.System.MouseKeyHook.HotKeys;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Windows.Forms;

namespace FovCalibrationTool.Keyboard.HotKeys
{
    public class HotKeysTracker
    {
        public async IAsyncEnumerable<HotKeyStatus> TrackAsync(IEnumerable<HotKey> hotKeysList, [EnumeratorCancellation] CancellationToken token)
        {
            var hotKeys = new HashSet<HotKey>(hotKeysList);
            var hotKeysChannel = Channel.CreateUnbounded<HotKeyStatus>();

            void hotKeyDownHandler(object sender, KeyEventArgs e)
            {
                hotKeyHandler(new HotKey(e.KeyCode, e.Modifiers), HotKeyDirection.Down);
            }

            void hotKeyUpHandler(object sender, KeyEventArgs e)
            {
                hotKeyHandler(new HotKey(e.KeyCode, e.Modifiers), HotKeyDirection.Up);
            }

            void hotKeyHandler(HotKey hotKey, HotKeyDirection direaction)
            {
                if (hotKeys.Contains(hotKey) == false)
                {
                    return;
                }

                var hotKeyStatus = new HotKeyStatus
                {
                    HotKey = hotKey,
                    HotKeyDirection = direaction
                };

                hotKeysChannel.Writer.TryWrite(hotKeyStatus);
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
