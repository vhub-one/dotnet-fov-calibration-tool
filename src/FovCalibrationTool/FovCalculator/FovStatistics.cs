
namespace FovCalibrationTool.FovCalculator
{
    public class FovStatistics
    {
        public double FovDeg { get; set; }
        public double FovDegAngleBased { get; set; }

        public double MoveDistancePer1Deg { get; set; }
        public double MoveDistancePer360Deg { get; set; }
        public double MoveDistancePerFovDeg { get; set; }
        public double MoveDistancePerViewPortDeg { get; set; }

        public double ViewPortDeg { get; set; }
        public double ViewPortWidth { get; set; }

        public double MoveDistancePer1DegSensBased { get; set; }
        public double MoveDistancePer360DegSensBased { get; set; }
        public double MoveDistancePerFovDegSensBased { get; set; }
        public double MoveDistancePerFovDegAngleBased { get; set; }
    }
}
