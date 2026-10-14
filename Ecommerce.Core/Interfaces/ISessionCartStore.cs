using System.Collections.Generic;
using Ecommerce.Core.Common;

namespace Ecommerce.Core.Interfaces
{
    /// <summary>
    /// Abstraction over the HTTP session cart state. The concrete implementation lives in
    /// Ecommerce.Web (HttpSessionCartStore) so that Ecommerce.Services never touches System.Web —
    /// "the session key lives in the Web layer, the cart logic lives in the service layer".
    /// </summary>
    public interface ISessionCartStore
    {
        /// <summary>Stable cart id: a session-scoped GUID (anonymous) or the user id (signed in).</summary>
        string GetOrCreateCartId();

        /// <summary>Identity user id of the current visitor, or null when anonymous.</summary>
        string UserId { get; }

        /// <summary>Current cart lines (ids + quantity only — no entity data).</summary>
        IList<CartLine> GetLines();

        void SetLines(IList<CartLine> lines);

        void ClearLines();
    }
}
