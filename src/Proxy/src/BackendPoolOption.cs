namespace Microsoft.AspNetCore.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text;

    public class BackendPoolOption
    {
        private int i = 0;

        public BackendPoolOption()
        {
        }

        public BackendPoolOption(ProxyOptions option)
        {
            this.Options = new ProxyOptions[] { option };
        }

        public ProxyOptions[] Options { get; set; }

        public ProxyOptions Pick()
        {
            var res = this.Options[i];
            i = (i + 1) % Options.Length;
            return res;
        }
    }
}
