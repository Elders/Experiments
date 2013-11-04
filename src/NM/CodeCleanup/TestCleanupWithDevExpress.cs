using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeCleanup
{
    public class TestCleanupWithDevExpress
    {
        private int a = 0;

        public TestCleanupWithDevExpress()
        {
            aaaa(new { Person = string.Empty, Age = 1 });
            var asd = string.Empty;
        }

        public int MyProperty { get; set; }

        private void MethodName()
        {
            var list = new List<int>();
            var result = from number in list
                         where (from num in list
                                where num == 2
                                select num).Any()
                         select number;
            foreach (var item in list)
            {
            }
        }

        private void TestUsings()
        {
            using (var stream = new MemoryStream())
            {
            }
            using (var stream1 = new MemoryStream())
            using (var stream2 = new MemoryStream())
            {
            }
        }

        public int aaaa(object asd)
        {
            var person = new { };
            var i = 0;
            return i++;
        }
    }
}
