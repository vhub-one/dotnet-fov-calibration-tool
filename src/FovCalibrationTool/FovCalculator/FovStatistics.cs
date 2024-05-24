
namespace FovCalibrationTool.FovCalculator
{
    public class FovStatistics
    {
        public double FovDeg { get; set; }
        public double FovDegAngleBased { get; set; }

        public double PointsPer1Deg { get; set; }
        public double PointsPer360Deg { get; set; }
        public double PointsPerFovDeg { get; set; }
        public double PointsPerViewPortDeg { get; set; }

        public double ViewPortDeg { get; set; }
        public double ViewPortWidth { get; set; }

        public double PointsPer1DegSensBased { get; set; }
        public double PointsPer360DegSensBased { get; set; }
        public double PointsPerFovDegSensBased { get; set; }
        public double PointsPerFovDegAngleBased { get; set; }
    }
}
