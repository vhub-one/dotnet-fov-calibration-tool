using Common.Hosting.Configuration;
using FovCalibrationTool.CalibrationTool;
using FovCalibrationTool.Keyboard.HotKeys;
using FovCalibrationTool.Mouse.MovementManager;
using FovCalibrationTool.Mouse.MovementTracker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

namespace FovCalibrationTool
{
    internal partial class ToolBootstrap
    {
        static void InitDefaultCommand(Command command)
        {
            command.Description = "Fov Calibration Tool";

            command.SetHandler(context =>
                HandleCommandAsync(context, ConfigureFovCalibrationToolHost)
            );
        }

        static void ConfigureFovCalibrationToolHost(HostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                #region [MouseMovementTracker]

                services.ConfigureByName<MouseMovementTrackerOptions>();
                services.AddSingleton<MouseMovementTracker>();

                #endregion

                #region [MouseMovementManager]

                services.AddSingleton<MouseMovementManager>();

                #endregion

                #region [HotKeysTracker]

                services.AddSingleton<HotKeysTracker>();

                #endregion

                services.ConfigureByName<CalibrationToolOptions>(required: true);
                services.AddHostedService<CalibrationToolService>();
            });
        }
    }
}
