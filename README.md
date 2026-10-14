# Legacy eCommerce Platform (class-plan specification)

A four-project ASP.NET MVC 5 solution on **.NET Framework 4.7** implementing the full
eCommerce class plan: catalog with category filtering and image galleries, an
AJAX session cart with a live mini-cart, a three-step checkout, ASP.NET Identity
accounts with order history, and EF6 Database First (EDMX) against SQL Server Express.

**Status: built, database seeded/verified, and functionally tested end-to-end over HTTP
(42/42 automated checks passing — see [Test evidence](#test-evidence)).**

---

## What's been implemented (improvement pass)

### 1. Header / navigation (shared `_Layout.cshtml` on every page)

* **Legacy Store** brand with icon on the left; **Home**, **Catalog** and a live
  **Categories** dropdown (server-rendered `Product/NavCategories` child action with
  per-category product-count badges).
* Inline **search box** in the navbar (plain GET to `/Product?q=…` — works without JS).
* **Cart pill** on the right: icon, label, green live-count badge and running total —
  refreshed in place by the existing MiniCart AJAX after every add/update/remove.
* **Account menu**: authenticated users get a dropdown (username → *My orders*,
  *Sign out*); anonymous users get **Log in** / **Register**.
* Standard Bootstrap 3 collapse for mobile (`#site-nav`), full-width search and cart
  rows on small screens.
* Dead files removed: `Views\Shared\_Header.cshtml` (referenced a non-existent
  `LogOff` action), `Views\Shared\_Footer.cshtml`, `Scripts\shop.js` (never bundled).

### 2. Real product images

* All 10 generated placeholders replaced with **genuine product photographs**
  (laptops, phones, earbuds, headphones, charger, keyboard) downloaded from
  **Wikimedia Commons** under CC0 / CC BY / CC BY-SA licenses and stored **locally**
  at the existing paths — no DB changes, no external URLs at runtime.
* Uniform **1000×750** canvas (white padding, JPEG q85), `object-fit: contain`
  everywhere, `loading="lazy"` + explicit dimensions on card images, and an
  `onerror` fallback to `Content\images\products\placeholder.png` for missing files.
* Full attribution (file, author, license, source URL) in
  **`Content\images\CREDITS.txt`**; stale `.png` twins of the old placeholders deleted.

### 3. Storefront

* **Homepage**: dark hero with headline + catalog search + CTAs, an honest feature
  strip, **category cards** (icon, name, count) and the featured grid
  (`HomeIndexViewModel`).
* **Catalog toolbar**: sort (newest / name A–Z / price ↑ / price ↓), **min/max price
  filters**, Apply + reset. It is a real GET form (works without JavaScript); with JS
  it re-filters `#product-list` via `/Product/Filter` without a page reload.
* **Loading / empty / error states**: spinner overlay while the AJAX filter runs,
  a friendly empty state with *Clear all filters*, an error banner on filter failure,
  and failure messages on cart update/remove AJAX.
* **Product detail**: breadcrumb, large gallery image, stock badge (*In stock* /
  *Only N left* / *Out of stock*), price box, variant + quantity form with in-flight
  button lock, description panel and a *You may also like* strip
  (`Product/RelatedProducts` child action).
* Repository/service layer extended (`Search` + `GetProductList` gained
  `sort` / `minPrice` / `maxPrice`) with a **sort whitelist** and price-bound
  normalization (negatives clamped, swapped bounds fixed) in the service.

### 4. Shopping workflow

* Add to cart, quantity update, remove and totals verified over HTTP;
  **all prices are re-hydrated server-side** — client-sent `unitPrice`/`Total`
  fields are ignored (tamper tests below).
* Full checkout (Address → Shipping → Payment → Confirmation) and order history
  re-verified; payment remains a **demo only** (banner on the payment step,
  `DEMO-…` reference, nothing transmitted).

---

## Stack (pinned by the specification)

| Component        | Version / choice                                        |
|------------------|---------------------------------------------------------|
| Runtime          | .NET Framework **4.7** (exact)                          |
| Web framework    | ASP.NET MVC 5 (5.2.7), Razor views, jQuery 3.4.1, Bootstrap 3 |
| Data access      | Entity Framework **6.4.4**, Database First / EDMX       |
| Database         | SQL Server Express `.\SQLEXPRESS` → `LegacyEcommerceDb` |
| Authentication   | OWIN + ASP.NET Identity **2.0.1** (cookie auth)         |
| DI               | Unity 5.11 (per-request child container)                |
| Build            | MSBuild 17.14 (VS 2022 Community)                       |

---

## Solution layout

```
LegacyEcommerce.sln
├── Ecommerce.Core      POCOs, ViewModels, interfaces (no dependencies)
├── Ecommerce.Data      EF6 EDMX (design-time), EcommerceDbContext, repositories
├── Ecommerce.Services  Catalog / Cart / Checkout / Account services + pricing
└── Ecommerce.Web       Thin controllers, Razor views, Unity composition root, OWIN
```

Layering rule enforced throughout: **Controller → Service → Repository → DbContext**.
Repositories are the only code that touches the database; views bind **ViewModels only**
(18 of them in `Ecommerce.Core\ViewModels`).

### Key design decisions

* **EF6 Database First**: the EDMX model lives in `Ecommerce.Data` (embedded via
  `EntityDeploy`) for the Database-First workflow. At runtime the fluent
  `EcommerceDbContext` (`base("name=EcommerceDb")`) maps the same POCO shapes; both
  `EcommerceDb` and `EcommerceIdentityDb` connection strings point at
  `.\SQLEXPRESS\LegacyEcommerceDb` with integrated security. Both EF database
  initializers are disabled — the reviewed SQL script owns the schema.
* **Unity DI**: `App_Start\UnityConfig.cs` registers everything, creates one child
  container per HTTP request (`Application_BeginRequest`/`EndRequest`) so exactly one
  `EcommerceDbContext` lives per request (`HierarchicalLifetimeManager`), and a small
  hand-written `UnityDependencyResolver` (MVC 5 + Unity 5.11 has no maintained bridge
  package). Resolution failures are surfaced as wrapped exceptions instead of silent
  `null`s.
* **Identity 2.0.1**: this pinned version predates `SignInManager` (added in 2.1), so
  `ApplicationSignInManager` in `IdentityModels.cs` is hand-rolled over
  `UserManager` + the OWIN cookie middleware (password check, lockout, sign-in).
* **Anti-forgery everywhere**: `[ValidateAntiForgeryToken]` on every POST action;
  for AJAX posts, `withToken()` in `Scripts\site.js` injects the form-field token
  (MVC 5.2.7 does not read header tokens).
* **Session cart** is the source of truth for cart contents; the MiniCart updates via
  AJAX after every add/update/remove.
* **Checkout wizard state** lives in `Session` (`CheckoutAddress`,
  `CheckoutShippingMethod`); confirmation passes through `TempData`. Payment is a
  **demo only** — no gateway, nothing transmitted; a local `DEMO-<timestamp>-<hex>`
  reference is stored.

---

## Build and run

### 1. Restore & build

```bat
msbuild LegacyEcommerce.sln /t:Rebuild /p:Configuration=Debug
```

(Package restore: `nuget restore LegacyEcommerce.sln` or build inside Visual Studio.
28 packages restore to the solution-local `packages\` folder.)

### 2. Database (already executed on this machine)

```bat
sqlcmd -S .\SQLEXPRESS -E -b -i "database\01_Create_LegacyEcommerceDb.sql"
```

Creates 14 tables, indexes, 17 foreign keys, seeds 5 categories / 10 products /
10 images / 7 variants / 2 roles, and creates login `legacy_app_user`
(`db_datareader` + `db_datawriter`). **On this machine the script has already been
run and verified — no action needed.**

### 3. Run

```bat
iisexpress.exe /path:"<solution-folder>\Ecommerce.Web" /port:50241
```

Open **http://localhost:50241/**

> ASP.NET loads assemblies from the flat `Ecommerce.Web\bin\` folder (WAP `OutputPath`
> is set to `bin\` for both configurations). After a rebuild the app domain recycles
> automatically — no IIS Express restart needed.

---

## Security notes

* `[Authorize]` on `CheckoutController` (class level) and order-history actions;
  anonymous requests are redirected to `/Account/Login?ReturnUrl=…`.
* Anti-forgery token required on every POST (verified: token-less POST →
  `HttpAntiForgeryException`).
* Views receive ViewModels only; no domain entities cross into `Views\`.
* No real credentials anywhere; `Web.config` carries local integrated-security
  connection strings only.
* Identity password policy: min 6, digit, upper + lower, non-letter-or-digit;
  5 failed attempts → 5-minute lockout.

---

## Test evidence

Automated end-to-end HTTP suite (PowerShell `HttpWebRequest` client: cookie jar,
manual redirects, anti-forgery tokens captured from rendered pages) —
**42 checks, 42 passing** against the running site after a clean Debug *and* Release
rebuild:

* **Pages & navigation** (11): home hero/category cards/featured, catalog toolbar,
  product detail (breadcrumb, buy box, related), cart page; **all 10 nav links
  (incl. category dropdown) resolve 200**; **no broken images** (9 unique image URLs
  across home/catalog/detail — all `200 image/*`); navbar search `q=laptop` and
  `category=2` filters return the right products.
* **Catalog filters**: `sort=price-asc|price-desc|name` orders verified against SQL,
  `priceMin=100&priceMax=600` returns exactly the 3 in-range products,
  hostile `sort` values fall back to *newest* (and raw `<script>` input is blocked
  by ASP.NET request validation, same as the pre-existing `q` parameter).
* **AJAX cart** (10): add qty 2 → JSON `success` + server price `$2,598.00`;
  MiniCart badge + total; qty update; **tampered `unitPrice=0.01` ignored**
  (subtotal stays `$1,299.00`); add/remove second line → `$1,448.00` → back;
  **token-less POST rejected (500)**.
* **Auth & authorization** (6): anonymous `/Checkout/Address` and `/Account/Orders`
  → 302 login; wrong password → “Invalid login attempt”; duplicate email →
  “already taken”; register → 302 + signed-in header (*My orders*, *Sign out*).
* **Checkout** (9): Address → Shipping (summary shows `$2,598.00`) → Payment
  (subtotal 2598.00 + tax 214.34 + shipping 0.00 = **2812.34**) → Confirmation;
  **tampered `Total=0.01` ignored — confirmed total = 2812.34**; order
  `LEG-20261013-…` persisted, demo banner shown, cart emptied, history matches.
* **Persistence** (4): logout → 302 and pages gated again; re-login →
  order still in history.
* Builds: **Debug 0 errors/0 warnings, Release 0 errors/0 warnings** (full solution
  rebuild); `node --check Scripts\site.js` passes.

Earlier manual pass additionally covered the gallery/variants UI, register →
`Customers` profile + `Customer` role + auto sign-in, and seeded data verification.

---

## Known limitations (intentional / spec-driven)

1. **Demo payment only** — `DEMO-<hex>` reference, no gateway integration.
2. **Anonymous cart orphan rows** — rows written to `CartItems` by anonymous sessions
   can remain after sign-in; the session is the cart's source of truth (documented
   design, harmless).
3. **Pinned legacy packages** carry NuGet advisories (NU1902/NU1903) — versions are
   spec-pinned and deliberately not upgraded.
4. **`Customer` role assignment** on register is wrapped in try/catch so registration
   still succeeds if the SQL seed hasn't run yet.
5. **Smoke-test data** in the local demo DB: a handful of `…@example.com` users and
   a few demo orders created during testing (all fake credentials; safe to delete).
6. **AJAX catalog filter state is not reflected in the address bar** — server-rendered
   URLs (`/Product?category=&q=&sort=&priceMin=&priceMax=&page=`) work and are
   linkable, but after client-side filtering a browser refresh returns to whatever
   state the current URL encodes.
7. **One photo per product** — the detail gallery renders N images if
   `ProductImages` rows exist; the seed provides a single (now real) photograph each.
8. **Responsive layout verified through markup/CSS only** — no interactive browser
   was attached in this session, so breakpoints follow standard Bootstrap 3
   behaviour but were not screenshot-tested.
9. **Featured products**: the seed marks 3 of 10 products `IsFeatured = 1`, so the
   home page shows a 3-card featured row (data intentionally left untouched).

## Troubleshooting

* *“Could not load Unity.Abstractions / System.Runtime.CompilerServices.Unsafe”* —
  stale files in the flat `bin\`; rebuild the solution (binding redirects in
  `Web.config` cover Unity 5.11.7/5.11.11 and Unsafe 4.0.4.1).
* *Razor “does not contain a definition for …”* — view namespaces live in
  `Ecommerce.Web\Views\Web.config` (`Ecommerce.Web.Infrastructure`,
  `System.Web.Optimization`, `Microsoft.AspNet.Identity` are all imported there).
