using System.Collections.Generic;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> for strings that is case insensitive
    /// </summary>
    internal class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x.ToUpperInvariant().Equals(y.ToUpperInvariant());
        }

        public int GetHashCode(string obj)
        {
            return obj.ToUpperInvariant().GetHashCode();
        }
    }
}