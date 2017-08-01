// AngularJS services
angular.module('appSrvc', ['ngMaterial'])
    .service('$appUtil', ['$http', '$mdUtil', '$mdSidenav', '$timeout', '$mdDialog', function ($http, $mdUtil, $mdSidenav, $timeout, $mdDialog) {
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
            var scrollContentEl = mainContentArea.querySelector('[md-scroll-y]');
            $mdUtil.animateScrollTo(scrollContentEl, 0, 200);
        }

        this.openMenu = function () {
            $timeout(function () { $mdSidenav('left').open(); });
        }

        this.appConfirm = function (ev, title, textContent, yesText, noText, callback) {
            if (typeof yesText === 'undefined' || yesText === '') { okText = '确定'; }
            if (typeof noText === 'undefined' || noText === '') { okText = '取消'; }
            var confirmDialog = $mdDialog.confirm()
                  .title(title)
                  .textContent(textContent)
                  .ariaLabel('confirm dialog')
                  .targetEvent(ev)
                  .ok(yesText)
                  .cancel(noText);
            confirmDialog.skipHide = true;
            $mdDialog.show(confirmDialog).then(function () {
                if (callback) {
                    callback();
                }
            }, function () {
                
            });
        };

        this.appAlert = function (ev, title, textContent, okText) {
            if (typeof okText === 'undefined' || okText === '') { okText = '好'; }
            var alertDialog = $mdDialog.alert()
                .parent(angular.element(document.querySelector('#popupContainer')))
                .clickOutsideToClose(true)
                .title(title)
                .textContent(textContent)
                .ariaLabel('alert dialog')
                .ok(okText)
                .targetEvent(ev)
            alertDialog.skipHide = true;
            $mdDialog.show(alertDialog);
        };

        this.getBalance = function (callback) {
            $http.get('/Account/GetBalance').then(function (res) {
                if (callback) { callback(res.data); }
            });
        }

        this.checkIDCard = function (ID) {
            if (typeof ID !== 'string') return false;
            var city = { 11: "北京", 12: "天津", 13: "河北", 14: "山西", 15: "内蒙古", 21: "辽宁", 22: "吉林", 23: "黑龙江 ", 31: "上海", 32: "江苏", 33: "浙江", 34: "安徽", 35: "福建", 36: "江西", 37: "山东", 41: "河南", 42: "湖北 ", 43: "湖南", 44: "广东", 45: "广西", 46: "海南", 50: "重庆", 51: "四川", 52: "贵州", 53: "云南", 54: "西藏 ", 61: "陕西", 62: "甘肃", 63: "青海", 64: "宁夏", 65: "新疆", 71: "台湾", 81: "香港", 82: "澳门", 91: "国外" };
            var birthday = ID.substr(6, 4) + '/' + Number(ID.substr(10, 2)) + '/' + Number(ID.substr(12, 2));
            var d = new Date(birthday);
            var newBirthday = d.getFullYear() + '/' + Number(d.getMonth() + 1) + '/' + Number(d.getDate());
            var currentTime = new Date().getTime();
            var time = d.getTime();
            var arrInt = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];
            var arrCh = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];
            var sum = 0, i, residue;

            if (!/^\d{17}(\d|x)$/i.test(ID)) return false;
            if (city[ID.substr(0, 2)] === undefined) return false;
            if (time >= currentTime || birthday !== newBirthday) return false;
            for (i = 0; i < 17; i++) {
                sum += ID.substr(i, 1) * arrInt[i];
            }
            residue = arrCh[sum % 11];
            if (residue !== ID.substr(17, 1)) return false;

            return true;
        }
    }]);
