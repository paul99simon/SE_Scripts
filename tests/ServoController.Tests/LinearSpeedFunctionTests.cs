namespace SE_Scripts.ServoController.Tests;

public class LinearAcceleration
{
    [Theory]
    [InlineData(0, 30, 0, 0,    "distance = 0 => minSpeed")]
    [InlineData(0, 30, 360, 30, "distance = 360 => maxSpeed")]
    [InlineData(0, 30, -360, -30, "distance = -360 => -maxSpeed")]
    [InlineData(2, 28, 0, 2,    "distance = 0 => minSpeed")]
    [InlineData(2, 28, 360, 28, "distance = 360 => maxSpeed")]
    [InlineData(2, 28, -360, -28, "distance = -360 => -maxSpeed")]
    public void LinearSpeedTest(float minSpeed, float maxSpeed, float distance, float expected, string description)
    {
        Program.LinearSpeedFunction speedFunction = new Program.LinearSpeedFunction(minSpeed, maxSpeed);
        float result = speedFunction.calculateSpeed(distance);
        Assert.Equal(expected, result, 0.1);
    }   
    
    [Theory]
    [InlineData(-10,   0, 0, "minSpeed < 0")]
    [InlineData( 31,   0, 0, "minSpeed > 30")]
    [InlineData(  0, -10, 0, "maxSpeed < 0")]
    [InlineData(  0,  31, 0, "maxSpeed > 30")]
    public void LinearSpeedTest_Throws(float minSpeed, float maxSpeed, float distance, string description)
    {
        Assert.ThrowsAny<ArgumentException>
        (
            () => {
                Program.LinearSpeedFunction speedFunction = new Program.LinearSpeedFunction(minSpeed, maxSpeed);
            } 
        );
    }   
}