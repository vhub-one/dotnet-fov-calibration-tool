using FovCalibrationTool.Keyboard.HotKeys;
using Gma.System.MouseKeyHook;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace FovCalibrationTool.FovCalculator
{
    public class CalculatorActionController
    {
        private readonly HotKeysTracker _hotKeysTracker;
        private readonly IOptions<CalcualtorActionOptions> _optionsAccessor;

        public CalculatorActionController(HotKeysTracker hotKeysTracker, IOptions<CalcualtorActionOptions> optionsAccessor)
        {
            _hotKeysTracker = hotKeysTracker;
            _optionsAccessor = optionsAccessor;
        }

        public async IAsyncEnumerable<CalculatorAction> TrackActionAsync([EnumeratorCancellation] CancellationToken token)
        {
            var options = _optionsAccessor.Value;

            if (options == null ||
                options.HotKeys == null)
            {
                throw new InvalidOperationException();
            }

            var hotKeys = options.HotKeys.ToDictionary(
                p => p.Key,
                p => Sequence.FromString(p.Value)
            );

            var hotKeysSequences = hotKeys.GroupBy(p => p.Value).ToDictionary(
                g => g.Key,
                g => g.Select(p => p.Key).ToList()
            );

            var hotKeyStatusStream = _hotKeysTracker.TrackAsync(hotKeysSequences.Keys, token);

            await foreach (var sequence in hotKeyStatusStream)
            {
                var actionsExist = hotKeysSequences.TryGetValue(sequence, out var actions);

                if (actionsExist)
                {
                    foreach (var action in actions)
                    {
                        yield return action;
                    }
                }
            }
        }
    }
}
