/* ============================================================================
 * Legacy Store — site.js
 * Global jQuery setup + the three AJAX flows required by the spec:
 *   1. every AJAX POST carries the anti-forgery token in the
 *      "RequestVerificationToken" header (AntiForgeryConfig.HeaderName);
 *   2. add-to-cart / update / remove return JSON;
 *   3. the header MiniCart is refreshed with .load() after each change;
 *      catalog filtering/pagination re-renders the _ProductList partial.
 * ========================================================================== */
var LegacyStore = (function ($) {
    'use strict';

    /* ---------------------------------------------------------------- helpers */

    function antiForgeryToken() {
        return $('#anti-forgery-token input[name="__RequestVerificationToken"]').val()
            || $('input[name="__RequestVerificationToken"]').first().val();
    }

    // Global setup: the token header is attached to every jQuery AJAX call.
    $.ajaxSetup({
        beforeSend: function (xhr) {
            var token = antiForgeryToken();
            if (token) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            }
        }
    });

    function refreshMiniCart() {
        var url = $('#mini-cart').data('url');
        if (url) {
            $('#mini-cart').load(url);
        }
    }

    function money(value) {
        return '$' + Number(value).toFixed(2);
    }

    // The pinned stack (MVC 5.2.7 + WebPages 3.2.7) only validates the anti-forgery
    // token from the FORM FIELD, so every AJAX POST must carry it in its data.
    // (The RequestVerificationToken header above is kept for forward-compat.)
    function withToken(data) {
        var token = antiForgeryToken();
        if (token && data && typeof data === 'object') {
            data.__RequestVerificationToken = token;
        }
        return data;
    }

    function showMessage($target, text, ok) {
        $target
            .removeClass('alert-success alert-danger')
            .addClass(ok ? 'alert-success' : 'alert-danger')
            .text(text)
            .show();
    }

    /* ------------------------------------------------------ add-to-cart (JSON) */

    function addToCart(data, $feedback, url) {
        if (!url) {
            url = $('#add-to-cart-form').data('url');
        }
        if (!url && $feedback && $feedback.length) {
            url = $feedback.data('url');
        }
        if (!url) {
            url = '/Cart/Add';
        }

        var request = $.ajax({
            url: url,
            type: 'POST',
            data: withToken(data),
            dataType: 'json'
        }).done(function (res) {
            if (res.success) {
                refreshMiniCart();
                if ($feedback && $feedback.length) {
                    showMessage($feedback, 'Added to cart — ' + res.itemCount + ' item(s), ' + money(res.subTotal), true);
                }
            } else if ($feedback && $feedback.length) {
                showMessage($feedback, res.message || 'Could not add the item.', false);
            }
        }).fail(function () {
            if ($feedback && $feedback.length) {
                showMessage($feedback, 'Request failed — please try again.', false);
            }
        });
        return request; // callers can show/disable a button while the request is in flight
    }

    /* ------------------------------------------------------- product detail page */

    function initProductDetail() {
        var $select = $('#variantId');
        var $price = $('#product-price');
        var $qty = $('#quantity');
        var $stockNote = $('#stock-note');

        if ($select.length && $price.length) {
            var basePrice = parseFloat($select.data('base-price')) || 0;

            function applyVariant() {
                var $opt = $select.find('option:selected');
                var adjustment = parseFloat($opt.data('adjustment')) || 0;
                var stock = parseInt($opt.data('stock'), 10) || 0;
                $price.text(money(basePrice + adjustment));
                $qty.attr('max', stock > 0 ? stock : 0);
                $stockNote.text(stock > 0 ? stock + ' in stock' : 'Out of stock');
            }

            $select.on('change', applyVariant);
            applyVariant();
        }

        var $form = $('#add-to-cart-form');
        if ($form.length) {
            $form.on('submit', function (e) {
                e.preventDefault();
                var $btn = $('#add-to-cart-button');
                var request = addToCart($form.serialize(), $('#add-to-cart-result'));
                if (request && request.always && $btn.length) {
                    $btn.prop('disabled', true);
                    request.always(function () {
                        $btn.prop('disabled', false);
                    });
                }
            });
        }
    }

    /* --------------------------------------------------- product cards (grid) */

    function initProductCards() {
        // Delegated: cards may be re-rendered by the AJAX filter.
        $(document).on('click', '.btn-add-to-cart', function () {
            var $btn = $(this);
            addToCart({ productId: $btn.data('product-id'), quantity: 1 }, $(), $btn.data('url'));
            $btn.blur();
            var original = $btn.html();
            $btn.html('<span class="glyphicon glyphicon-ok"></span> Added').prop('disabled', true);
            setTimeout(function () {
                $btn.html(original).prop('disabled', false);
            }, 1200);
        });
    }

    /* ------------------------------------------------------- catalog AJAX filter */

    // Full filter state lives on #product-list (data attributes) so every entry point
    // (sidebar search, category tree, toolbar, pager) can rebuild the request.
    function filterState() {
        var $list = $('#product-list');
        return {
            category: $list.attr('data-category') || '',
            q: $list.attr('data-q') || '',
            sort: $list.attr('data-sort') || 'newest',
            min: $list.attr('data-min') || '',
            max: $list.attr('data-max') || ''
        };
    }

    function toolbarState() {
        return {
            sort: $('#filter-sort').val() || 'newest',
            min: $('#filter-min').val() || '',
            max: $('#filter-max').val() || ''
        };
    }

    function loadProductList(state, page) {
        var $list = $('#product-list');
        if (!$list.length) {
            return;
        }
        state = state || filterState();
        page = page || 1;

        $('#product-list-error').hide().text('');
        $list.addClass('is-loading'); // CSS shows the spinner overlay

        $.ajax({
            url: $list.data('filter-url'),
            type: 'GET',
            data: {
                category: state.category || null,
                q: state.q || '',
                sort: state.sort || 'newest',
                priceMin: state.min || null,
                priceMax: state.max || null,
                page: page
            },
            dataType: 'html',
            cache: false
        }).done(function (html) {
            $list.html(html)
                .attr('data-category', state.category || '')
                .attr('data-q', state.q || '')
                .attr('data-sort', state.sort || 'newest')
                .attr('data-min', state.min || '')
                .attr('data-max', state.max || '');
        }).fail(function () {
            $('#product-list-error')
                .text('The catalog could not be loaded — check your connection and try again.')
                .show();
        }).always(function () {
            $list.removeClass('is-loading');
        });
    }

    function initCatalogFilter() {
        // Search box (sidebar) — AJAX instead of full page reload.
        $(document).on('submit', '#catalog-search', function (e) {
            e.preventDefault();
            var state = filterState();
            state.q = $(this).find('input[name="q"]').val() || '';
            loadProductList(state, 1);
        });

        // Toolbar (sort / price range) — AJAX filter, page resets to 1.
        $(document).on('submit', '#catalog-filter-form', function (e) {
            e.preventDefault();
            var state = filterState();
            var toolbar = toolbarState();
            state.sort = toolbar.sort;
            state.min = toolbar.min;
            state.max = toolbar.max;
            loadProductList(state, 1);
        });
        $(document).on('change', '#filter-sort', function () {
            $('#catalog-filter-form').trigger('submit');
        });

        // Category links — AJAX filter, heading follows the chosen category.
        $(document).on('click', '#category-tree a', function (e) {
            e.preventDefault();
            var href = $(this).attr('href') || '';
            var match = href.match(/category=(\d+)/);
            var state = filterState();
            state.category = match ? match[1] : '';
            state.q = '';
            $('#catalog-search input[name="q"]').val('');
            $('#catalog-heading').text($(this).text().replace(/\s*\d+$/, '').trim() || 'Catalog');
            loadProductList(state, 1);
        });

        // Pagination links inside the re-rendered partial.
        $(document).on('click', '.pager-link', function (e) {
            e.preventDefault();
            if ($(this).closest('li').hasClass('disabled') || $(this).closest('li').hasClass('active')) {
                return;
            }
            loadProductList(filterState(), $(this).data('page'));
        });
    }

    /* ------------------------------------------------------------- cart page */

    function initCartPage() {
        function recalcTotals() {
            var subtotal = 0;
            $('#cart-table tbody tr').each(function () {
                var qty = parseInt($(this).find('.cart-qty').val(), 10) || 0;
                var unit = parseFloat($(this).data('unit-price')) || 0;
                var line = qty * unit;
                subtotal += line;
                $(this).find('.line-total').text(money(line));
            });
            $('#cart-subtotal').text(money(subtotal));
        }

        $(document).on('change', '#cart-table .cart-qty', function () {
            var $row = $(this).closest('tr');
            var max = parseInt($row.data('max-stock'), 10) || 1;
            var qty = parseInt($(this).val(), 10) || 0;
            if (qty > max) {
                qty = max;
                $(this).val(qty);
            }

            $.ajax({
                url: '/Cart/Update',
                type: 'POST',
                data: withToken({
                    productId: $row.data('product-id'),
                    variantId: $row.data('variant-id') || null,
                    quantity: qty
                }),
                dataType: 'json'
            }).done(function (res) {
                if (res.success) {
                    if (qty <= 0) {
                        $row.remove();
                        recalcTotals();
                    } else {
                        recalcTotals();
                    }
                    refreshMiniCart();
                    if ($('#cart-table tbody tr').length === 0) {
                        window.location.reload(); // empty state + checkout button must reset
                    }
                } else {
                    showMessage($('#cart-ajax-status'), res.message || 'Update failed.', false);
                    window.setTimeout(function () { $('#cart-ajax-status').hide(); }, 3000);
                }
            }).fail(function () {
                showMessage($('#cart-ajax-status'), 'Update failed — please check your connection and try again.', false);
            });
        });

        $(document).on('click', '#cart-table .btn-remove-line', function () {
            var $row = $(this).closest('tr');
            $.ajax({
                url: '/Cart/Remove',
                type: 'POST',
                data: withToken({
                    productId: $row.data('product-id'),
                    variantId: $row.data('variant-id') || null
                }),
                dataType: 'json'
            }).done(function (res) {
                if (res.success) {
                    $row.remove();
                    recalcTotals();
                    refreshMiniCart();
                    if ($('#cart-table tbody tr').length === 0) {
                        window.location.reload();
                    }
                } else {
                    showMessage($('#cart-ajax-status'), res.message || 'Remove failed.', false);
                }
            }).fail(function () {
                showMessage($('#cart-ajax-status'), 'Remove failed — please check your connection and try again.', false);
            });
        });
    }

    /* ------------------------------------------------------------------- boot */

    $(function () {
        initProductCards();
        // Per-page initialisers are called inline from the views themselves.
    });

    return {
        initProductDetail: initProductDetail,
        initCatalogFilter: initCatalogFilter,
        initCartPage: initCartPage
    };
})(jQuery);
