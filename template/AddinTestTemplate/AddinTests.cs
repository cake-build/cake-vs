using System.IO;
using Xunit;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;

namespace $safeprojectname$
{
	public class AddinTests
    {
        [Fact]
        public void ShouldThrowIfFileNotExists()
        {
            // arrange
            var fixture = new CakeFixture(false);
            //act
            var result = Record.Exception(() => fixture.Run());
            //assert
            Assert.IsType(typeof(FileNotFoundException), result);
        }

        [Theory]
        [InlineData("./build.cake")]
        [InlineData("./deploy.cake")]
        public void ShouldIncludeFileName(string scriptFilePath)
        {
            //arrange
            var fixture = new CakeFixture(true, scriptFilePath);
            //act
            var result = fixture.Run();
            //result
            Assert.True(result.Args.Contains(scriptFilePath));
        }
    }
}
