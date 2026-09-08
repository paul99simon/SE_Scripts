
namespace SE_Scripts.ServoRotorController.Tests;

public class RASCFunctionTests
{
    [Theory]
    [InlineData(0, 30,    0,   0, "distance = 0 => minSpeed")]
    [InlineData(0, 30,  360,   0, "distance = 360 => minSpeed")]
    [InlineData(0, 30, -360,   0, "distance = -360 => minSpeed")]
    [InlineData(0, 30,  180,  30, "distance = 180 => maxSpeed")]
    [InlineData(0, 30, -180, -30, "distance = -180 => -maxSpeed")]
    [InlineData(2, 28,    0,   2, "distance = 0 => minSpeed")]
    [InlineData(2, 28,  360,   2, "distance = 360 => minSpeed")]
    [InlineData(2, 28, -360,  -2, "distance = -360 => -minSpeed")]
    [InlineData(2, 28,  180,  28, "distance = 180 => maxSpeed")]
    [InlineData(2, 28, -180, -28, "distance = -180 => -maxSpeed")]
    public void RASCFunctionTest(float minSpeed, float maxSpeed, float distance, float expected, string description)
    {
        Program.ISpeedFunction speedFunction = new Program.RASCFunction(minSpeed, maxSpeed);
        float result = speedFunction.calculateSpeed(distance);
        Assert.Equal(expected, result, 0.1);
    }   
    
    [Theory]
    [InlineData(-10,   0, 0, 0, 0, "minSpeed < 0")]
    [InlineData( 31,   0, 0, 0, 0, "minSpeed > 30")]
    [InlineData(  0, -10, 0, 0, 0, "maxSpeed < 0")]
    [InlineData(  0,  31, 0, 0, 0, "maxSpeed > 30")]
    [InlineData(0f, 15f, -0.1d, 0d,  0, "k must be in [0, infinity)")]
    [InlineData(0f, 15f, -10.0d, 0d, 0, "k must be in [0, infinity)")]
    [InlineData(0f, 15f, 10d, -1.0d, 0, "w must be in (-1, 1)")] // Exact lower edge
    [InlineData(0f, 15f, 10d, -1.5d, 0, "w must be in (-1, 1)")] // Below lower edge
    [InlineData(0f, 15f, 10d, 1.0d,  0, "w must be in (-1, 1)")]  // Exact upper edge
    [InlineData(0f, 15f, 10d, 1.5d,  0, "w must be in (-1, 1)")]  // Above upper edge
    public void RASCFunction_Throws(float minSpeed, float maxSpeed, float k, float w, float distance, string description)
    {
        Assert.ThrowsAny<ArgumentException>
        (
            () => {
                Program.ISpeedFunction speedFunction = new Program.RASCFunction(minSpeed, maxSpeed, k, w);
            } 
        );
    }   
}