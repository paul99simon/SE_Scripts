using SE_Scripts.ServoController;

namespace SE_Scripts.ServoController.Tests;

public class DistanceTests
{
    [Theory]
    [InlineData(0,0,float.MinValue, float.MaxValue, 0)]
    public void DistanceTest(float currentAngle, float targetAngle, float lowerLimit, float upperLimit, float expected)
    {
        float result = SE_Scripts.ServoController.Program.getDistance(currentAngle, targetAngle, lowerLimit, upperLimit);
        Assert.Equal(expected, result);
    }
}
