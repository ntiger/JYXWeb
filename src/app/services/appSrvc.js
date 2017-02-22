// AngularJS services
angular.module('appSrvc', ['ngMaterial'])
    .service('$appUtil', ['$http', '$mdUtil', '$mdSidenav', '$timeout', function ($http, $mdUtil, $mdSidenav, $timeout) {
        var $appUtil = this;

        // open a new window with 4/5 width and height of current window.
        this.openNewWindow = function (url) {
            var width = $(window).width() * 2 / 2.5;
            var height = $(window).height() * 2 / 2.5;
            if (navigator.appName == "Microsoft Internet Explorer") {
                window.open(url, "", "width=" + width + ", height=" + height + ",scrollbars=yes,resizable=yes,toolbar=yes,menubar=yes");
            }
            else {
                window.open(url, "", "width=" + width + ", height=" + height + ",scrollbars=yes,toolbar=yes,menubar=yes");
            }
        };

        // uri encode a string (i.e. space => %20)
        this.encodeUrl = function (str) {
            return encodeURIComponent(str);
        }

        this.firstToken = function self(topTicker, bottomTicker, type) {
            if (typeof bottomTicker !== 'undefined' && typeof type !== 'undefined') {
                var typeSymbol = type === 'Ratio' ? '/' : type === 'Spread' ? '-' : '';
                return self(topTicker) + ' ' + typeSymbol + ' ' + self(bottomTicker);
            }
            if (typeof topTicker === 'undefined') { return ''; }
            if (topTicker.lastIndexOf(' ') === -1) { return topTicker; }
            return topTicker.substring(0, topTicker.lastIndexOf(' '));
        }

        this.getSearchParameters = function () {
            var parameterString = window.location.search.substr(1);
            return parameterString != null && parameterString != "" ? transformToAssocArray(parameterString) : {};
        }

        this.transformToAssocArray = function (parameterString) {
            var params = {};
            var parameterArray = parameterString.split("&");
            for (var i = 0; i < parameterArray.length; i++) {
                var tmparr = parameterArray[i].split("=");
                params[tmparr[0]] = tmparr[1];
            }
            return params;
        }

        this.getKeys = function (obj) {
            if (!obj) {
                return [];
            }
            return Object.keys(obj);
        }

        this.contains = function (array, obj, property) {
            if (!obj) {
                return false;
            }
            for (var i = 0; i < array.length; i++) {
                if (array[i][property] === obj[property]) {
                    return true;
                }
            }
            return false;
        }

        this.getIndexByKeyValue = function (array, key, value) {
            for (var i = 0; i < array.length; i++) {
                if (array[i][key] == value) {
                    return i;
                }
            }
            return null;
        };

        // For Typeahead
        this.startsWith = function (state, viewValue) {
            return state.substr(0, viewValue.length).toLowerCase() == viewValue.toLowerCase();
        }

        this.isIE = function () {
            var myNav = navigator.userAgent.toLowerCase();
            return (myNav.indexOf('msie') != -1) ? parseInt(myNav.split('msie')[1]) : false;
        };

        this.convertImgToBase64URL = function (url, callback, outputFormat) {
            var img = new Image();
            img.crossOrigin = 'Anonymous';
            img.onload = function () {
                var canvas = document.createElement('CANVAS'),
                ctx = canvas.getContext('2d'), dataURL;
                canvas.height = img.height;
                canvas.width = img.width;
                ctx.drawImage(img, 0, 0);
                dataURL = canvas.toDataURL(outputFormat);
                callback(dataURL);
                canvas = null;
            };
            img.src = url;
        }

        this.getNumberArray = function (num) {
            return new Array(num);
        };

        this.orderByKey = function (data, key, type, asc) {
            if (typeof (data) === 'undefined' || typeof (key) === 'undefined') {
                return data;
            }
            var tmpData = data.slice(0);
            tmpData.sort(function (a, b) {
                var aValue = type === 'dateStr' ? new Date(a[key]) : a[key];
                var bValue = type === 'dateStr' ? new Date(b[key]) : b[key];
                if (aValue < bValue)
                    return asc ? -1 : 1;
                else if (aValue > bValue)
                    return asc ? 1 : -1;
                else
                    return 0;
            });
            return tmpData;
        };

        this.scrollTop = function ($event) {
            var mainContentArea = document.querySelector("[role='main']");
            var scrollContentEl = mainContentArea.querySelector('md-content[md-scroll-y]');
            $mdUtil.animateScrollTo(scrollContentEl, 0, 200);
        }

        this.openMenu = function () {
            $timeout(function () { $mdSidenav('left').open(); });
        }
    }]);
