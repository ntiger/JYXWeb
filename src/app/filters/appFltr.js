// AngularJS filters
angular.module('appFltr', [])

    .filter('percentage', ['$filter', function ($filter) {
        // Percentage filter
        return function (input, decimals) {
            if (typeof decimals === 'undefined') { decimals = 2; }
            return input === null || isNaN(input) ? '' : $filter('number')(input * 100, decimals) + '%';
        };
    }])
    .filter('percentagePlus', ['$filter', function ($filter) {
        // Percentage filter
        return function (input, decimals) {
            if (typeof decimals === 'undefined') { decimals = 2; }
            return input === null || isNaN(input) ? '' : (input > 0 ? '+' : '') + $filter('number')(input * 100, decimals) + '%';
        };
    }])

    .filter('round', ['$filter', function ($filter) {
        // Round filter
        return function (input, decimals, forceRound) {
            if (typeof decimals === 'undefined') { decimals = 2; }
            var str = '.' + Array(decimals + 1).join('0');
            return input === null || !input ? '' : forceRound ? $filter('number')(input, decimals) : $filter('number')(input, decimals).replace(str, '');
        };
    }])

    .filter('firstToken', function () {
        // Get first token for bloomberg ticker
        return function (ticker) {
            if (typeof ticker === 'undefined') { return ticker; }
            if (ticker.lastIndexOf(' ') === -1) { return ticker; }
            return ticker.substring(0, ticker.lastIndexOf(' '));
        }
    })

    .filter('noSpace', function () {
        // return string without space
        return function (str) {
            if (typeof str === 'undefined') { return ''; }
            return str.replace(/\s+/g, '');
        }
    })

    .filter('toLower', function () {
        // return lower case string
        return function (str) {
            return str.toLowerCase();
        }
    })

    .filter('toUpper', function () {
        // return upper case string
        return function (str) {
            return typeof str !== 'undefined' ? str.toUpperCase() : str;
        }
    })

    .filter('whose', function () {
        return function (user) {
            if (user === undefined) { return ""; }
            user = user.charAt(0).toUpperCase() + user.slice(1);
            if (user.charAt(user.length - 1) === 's') { user += '\''; }
            else { user += '\'s'; }
            return user;
        }
    })

    .filter('sumByKey', function () {
        // Sum the json array values by the given key, from start index to end index
        return function (data, key, start, end) {
            if (typeof (data) === 'undefined' || typeof (key) === 'undefined') {
                return 0;
            }
            if (typeof (start) === 'undefined') {
                start = 0;
            }
            if (typeof (end) === 'undefined') {
                end = data.length - 1;
            }
            var sum = 0;
            for (var i = end; i >= start; i--) {
                if (!isNaN(data[i][key])) {
                    sum += data[i][key];
                }
            }
            return sum;
        };
    })

    .filter('emptyToEnd', function () {
        return function (array, key) {
            if (!angular.isArray(array)) return;
            var present = array.filter(function (item) {
                return item[key];
            });
            var empty = array.filter(function (item) {
                return !item[key]
            });
            return present.concat(empty);
        };
    })

    .filter('removeSpace', function () {
        return function (str) {
            str = str.replace(/ /gi, '');
            return str;
        }
    })

    .filter('urlEncode', function () {
        return window.encodeURIComponent;
    })
    .filter('unique', function () {
        return function (items, filterOn) {
            if (filterOn === false) {
                return items;
            }
            if ((filterOn || angular.isUndefined(filterOn)) && angular.isArray(items)) {
                var hashCheck = {}, newItems = [];
                var extractValueToCompare = function (item) {
                    if (angular.isObject(item) && angular.isString(filterOn)) {
                        return item[filterOn];
                    } else {
                        return item;
                    }
                };
                angular.forEach(items, function (item) {
                    var valueToCheck, isDuplicate = false;
                    for (var i = 0; i < newItems.length; i++) {
                        if (angular.equals(extractValueToCompare(newItems[i]), extractValueToCompare(item))) {
                            isDuplicate = true;
                            break;
                        }
                    }
                    if (!isDuplicate) {
                        newItems.push(item);
                    }
                });
                items = newItems;
            }
            return items;
        };
    })

    .filter('average', function () {
        return function (objects, field) {
            if (typeof objects === 'undefined') { return 0; }
            var sum = 0;
            var total = objects.length;
            for (var i = 0; i < objects.length; i++) {
                if (isNaN(objects[i][field]) || objects[i][field] === '') {
                    total--;
                }
                else {
                    sum += typeof objects[i][field] === 'string' ? parseFloat(objects[i][field]) : objects[i][field];
                }
            }
            return sum / total;
        };
    })

    .filter('median', function () {
        return function (objects, field) {
            if (typeof objects === 'undefined') { return ''; }
            var arr = objects.slice(0);
            for (var i = arr.length - 1; i >= 0; i--) {
                if (isNaN(arr[i][field]) || arr[i][field] == '') {
                    arr.splice(i, 1);
                }
            }
            if (arr.length === 0) { return ''; }
            arr.sort(function (a, b) {
                if (isNaN(a[field]) || isNaN(b[field])) {
                    return 0;
                }
                var value1 = typeof a[field] === 'string' ? parseFloat(a[field]) : a[field];
                var value2 = typeof b[field] === 'string' ? parseFloat(b[field]) : b[field];
                return (value1 > value2) ? 1 : ((value2 > value1) ? -1 : 0);
            });
            var size = arr.length;
            var mid = Math.floor(size / 2);
            var median = (size % 2 != 0) ? arr[mid][field] :
                (typeof arr[mid][field] === 'string' ? (parseFloat(arr[mid][field]) + parseFloat(arr[mid - 1][field])) / 2 : (arr[mid][field] + arr[mid - 1][field]) / 2);
            return median;
        };
    })

    .filter('percentUp', function () {
        return function (objects, field) {
            if (typeof objects === 'undefined') { return 0; }
            var count = 0;
            for (var i = 0; i < objects.length; i++) {
                if (objects[i][field] > 0) { count++; };
            }
            return count / objects.length;
        };
    })

    .filter('loop', function () {
        return function (input, total) {
            total = parseInt(total);
            for (var i = 0; i < total; i++)
                input.push(i);
            return input;
        };
    })

    .filter('formatDateTime', function () {
        return function (input) {
            var date = eval("new " + input.replace(/\//g, '') + ";");
            return (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear() + " " +
                date.getHours() + ':' + ((date.getMinutes() < 10) ? ("0" + date.getMinutes()) : (date.getMinutes()));
        };
    });



