namespace SE_Scripts.ServoRotorController.Tests;

public class GetDistanceTests
{
    [Theory]
    //Unlimited Mode
    [InlineData(45,  45,float.MinValue, float.MaxValue,    0,   "Unlimited Mode: No rotation")]
    [InlineData(45,  90,float.MinValue, float.MaxValue,  -45,   "Unlimited Mode: CounterClockwise")]
    [InlineData(90,  45,float.MinValue, float.MaxValue,   45,   "Unlimited Mode: Clockwise")]
    [InlineData(45, 315,float.MinValue, float.MaxValue,   90,   "Unlimited Mode: Clockwise across 0")]
    [InlineData(315, 45,float.MinValue, float.MaxValue,  -90,   "Unlimited Mode: Counter Clockwise across 0")]
    [InlineData(90, 269,float.MinValue, float.MaxValue, -179,   "Unlimited Mode: CounterClockwise")]
    [InlineData(269, 90,float.MinValue, float.MaxValue,  179,   "Unlimited Mode: Clockwise")]
    [InlineData(90, 271,float.MinValue, float.MaxValue,  179,   "Unlimited Mode: Clockwise across 0")]
    [InlineData(271, 90,float.MinValue, float.MaxValue, -179,   "Unlimited Mode: Counter Clockwise across 0")]
    [InlineData(0, 181,float.MinValue, float.MaxValue,   179,   "Unlimited Mode: Clockwise from 0")]
    [InlineData(0, 179,float.MinValue, float.MaxValue,  -179,   "Unlimited Mode: CounterClockwise from 0")]
    [InlineData(0, 180,float.MinValue, float.MaxValue,   180,   "Unlimited Mode: (Counter)-Clockwise to 0")]
    [InlineData(181, 0,float.MinValue, float.MaxValue,  -179,   "Unlimited Mode: Counter Clockwise to 0")]
    [InlineData(179, 0,float.MinValue, float.MaxValue,   179,   "Unlimited Mode: Clockwise to 0")]
    [InlineData(180, 0,float.MinValue, float.MaxValue,  -180,   "Unlimited Mode: (Counter)-Clockwise to 0")]
    //Limited Mode
    [InlineData(   0, -180, -360.5, 360.5, -180,   "Limited Mode: Counter-Clowckwise from 0")]
    [InlineData(-180,    0, -360.5, 360.5,  180,   "Limited Mode: Clowckwise to 0")]
    [InlineData(  45,  -45,    -90,    90,  -90,   "Limited Mode: Counter-Clowckwise across 0")]
    [InlineData( -45,   45,    -90,    90,   90,   "Limited Mode: Clowckwise across 0")]
    [InlineData( 135, -135,   -170,   170, -270,   "Limited Mode: Counter-Clowckwise across 0")]
    [InlineData(-135,  135,   -170,   170,  270,   "Limited Mode: Clowckwise across 0")]
    [InlineData( -90, -130,   -135,   -90,  -40,   "Limited Mode: Counter Clowckwise")]
    [InlineData(-130,  -90,   -135,   -90,   40,   "Limited Mode: Clowckwise")]
    [InlineData( 130,   90,     90,   135,  -40,   "Limited Mode: Counter Clowckwise")]
    [InlineData(  90,  130,     90,   135,   40,   "Limited Mode: Clowckwise")]
    //Mixed Mode
    [InlineData(-180,   90,   -190, float.MaxValue,   270,   "Mixed Mode: Clowckwise")]
    [InlineData(  90, -180,   -190, float.MaxValue,  -270,   "Mixed Mode: Counter Clowckwise")]
    [InlineData( 350,   10,      0, float.MaxValue,  -340,   "Mixed Mode: Counter-Clockwise due to Min limit")]
    [InlineData(  10,  350,      0, float.MaxValue,   340,   "Mixed Mode: Clockwise due to Min limit")]
    [InlineData( 180,  -90, float.MinValue, 190, -270,   "Mixed Mode: Counter-Clockwise due to Max limit")]
    [InlineData( -90,  180, float.MinValue, 190,  270,   "Mixed Mode: Clockwise due to Max limit")]
    [InlineData(-350,  -10, float.MinValue,   0,  340,   "Mixed Mode: Counter-Clockwise due to Min limit")]
    [InlineData( -10, -350, float.MinValue,   0, -340,   "Mixed Mode: Clockwise due to Min limit")]
    public void DistanceTest(float currentAngle, float targetAngle, float lowerLimit, float upperLimit, float expected, string description)
    {
        Program program = new Program();
        float result = program.getDistance(currentAngle, targetAngle, lowerLimit, upperLimit);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-10, 0, float.MinValue, float.MaxValue, "Unlimited Mode: currentAngle not in [0, 360)")]
    [InlineData(370, 0, float.MinValue, float.MaxValue, "Unlimited Mode: currentAngle not in [0, 360)")]
    [InlineData(0, -10, float.MinValue, float.MaxValue, "Unlimited Mode: targetAngle not in [0, 360)")]
    [InlineData(0, 370, float.MinValue, float.MaxValue, "Unlimited Mode: targetAngle not in [0, 360)")]
    [InlineData(360, 0, float.MinValue, float.MaxValue, "Unlimited Mode: currentAngle not in [0, 360)")]
    [InlineData(0, 360, float.MinValue, float.MaxValue, "Unlimited Mode: targetAngle not in [0,360)")]
    [InlineData(360.51, 0, -360, 360,   "Limited Mode:  currentAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(-360.51, 0, -360, 360,  "Limited Mode:  currentAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(360.5, 0, -360, 360,    "Limited Mode:  currentAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(-360.5, 0, -360, 360,   "Limited Mode:  currentAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(0, 360.51, -360, 360,   "Limited Mode:  targetAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(0, -360.51, -360, 360,  "Limited Mode:  targetAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(0, 360.5, -360, 360,    "Limited Mode:  targetAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(0, -360.5, -360, 360,   "Limited Mode:  targetAngle  not in [lowerLimit, upperLimit]")]
    [InlineData(0, 0, 360.51, 360.51,   "Limited Mode: lowerLimit and upperLimit  not in [-360.5, 360.5]")]
    [InlineData(0, 0, -360.51, -360.51, "Limited Mode: lowerLimit and upperLimit  not in [-360.5, 360.5]")]
    [InlineData(0, 0, -360.51, 360.51,  "Limited Mode: lowerLimit and upperLimit  not in [-360.5, 360.5]")]
    [InlineData(0, 0, 360.51, -360.51,  "Limited Mode: lowerLimit and upperLimit  not in [-360.5, 360.5]")]
    [InlineData(0, 0, 91, 90,           "Limited Mode: upperLimit < lowerLimit")]
    [InlineData(0, 0, -90, -91,         "Limited Mode: upperLimit < lowerLimit")]
    [InlineData(100, 0, -90, 90,        "Limited Mode: currentAngle > upperLimit")]
    [InlineData(0, 100, -90, 90,        "Limited Mode: targetAngle > upperLimit")]
    [InlineData(-100, 0, -90, 90,       "Limited Mode: currentAngle < lowerLimit")]
    [InlineData(0, -100, -90, 90,       "Limited Mode: targetAngle < lowerLimit")]
    [InlineData(-100, 0, -90, float.MaxValue,   "MixedMode: currentAngle < lowerLimit")]
    [InlineData(100, 0, float.MinValue, 90,     "MixedMode: currentAngle > upperLimit")]
    [InlineData(0, -100, -90, float.MaxValue,   "MixedMode: targetAngle < lowerLimit")]
    [InlineData(0, 100, float.MinValue, 90,     "MixedMode: targettAngle > upperLimit")]
    public void DistanceTest_Throws(float currentAngle, float targetAngle, float lowerLimit, float upperLimit, string description)
    {
        Program program = new Program();
        Assert.ThrowsAny<ArgumentException>
        (
            () => program.getDistance(currentAngle, targetAngle, lowerLimit, upperLimit)
        );
    }
}
