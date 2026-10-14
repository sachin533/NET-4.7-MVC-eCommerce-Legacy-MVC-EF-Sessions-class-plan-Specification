using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;
using Ecommerce.Core.Common;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNet.Identity;

namespace Ecommerce.Web.Infrastructure
{
    /// <summary>
    /// The Web layer owns the session (keys live here): the cart id + cart lines are stored
    /// in the InProc session (20 min, see web.config). Ecommerce.Services only sees this interface.
    /// CartId rule (matches the SQL comment): user id when signed in, session GUID when anonymous.
    /// </summary>
    public class HttpSessionCartStore : ISessionCartStore
    {
        private const string CartIdKey = "CartId";
        private const string CartLinesKey = "CartLines";

        public string UserId
        {
            get
            {
                IIdentity identity = HttpContext.Current.User.Identity;
                return identity.IsAuthenticated ? identity.GetUserId() : null;
            }
        }

        public string GetOrCreateCartId()
        {
            string userId = UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                return userId; // authenticated cart: keyed by the Identity user id
            }

            HttpSessionState session = Session();
            string cartId = session[CartIdKey] as string;
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = "S-" + Guid.NewGuid().ToString("N"); // anonymous session cart
                session[CartIdKey] = cartId;
            }
            return cartId;
        }

        public IList<CartLine> GetLines()
        {
            return Session()[CartLinesKey] as IList<CartLine> ?? new List<CartLine>();
        }

        public void SetLines(IList<CartLine> lines)
        {
            Session()[CartLinesKey] = lines;
        }

        public void ClearLines()
        {
            Session()[CartLinesKey] = null;
        }

        private static HttpSessionState Session()
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
            {
                throw new InvalidOperationException(
                    "HTTP session is not available. ISessionCartStore requires an ASP.NET session (in-proc).");
            }
            return context.Session;
        }
    }
}
