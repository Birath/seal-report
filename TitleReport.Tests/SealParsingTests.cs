using Bunit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitleReport.Tests
{
    public class SealParsingTests
    {
        [Fact]
        public void ParsesSealCorretly()
        {
            using var ctx = new TestContext();

            Utilities.SetupDefaultTestContext(ctx);

            var httpMock = ctx.Services.AddMockHttpClient();


        }

    }
}
