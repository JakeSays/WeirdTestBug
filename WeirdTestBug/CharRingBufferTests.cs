using Things;

namespace WeirdTestBug;

public class Tests
{
    [Test]
    public void ThisShouldntFail()
    {
        var buffer = new CharRingBuffer(26);

        buffer.Write("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        Assert.Multiple(() =>
        {
            Assert.That(buffer.ToString(), Is.EqualTo("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.That(buffer.CanWrite, Is.False);
            Assert.That(buffer.CanRead, Is.True);
            Assert.That(buffer.WriteSpaceAvailable, Is.EqualTo(0));
        });
        
    }
}
