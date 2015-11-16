using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandBar
{
    public interface ISuggestionsProvider
    {
        IEnumerable GetSuggestions(string filter);
    }
}
