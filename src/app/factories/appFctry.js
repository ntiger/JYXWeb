angular.module('appFctry', [])
    /**
    * Angular factory for file reader, i.e. image preview as data uri
    */
    .factory("$fileReader", ["$q", "$log", function ($q, $log) {
        var onLoad = function (reader, deferred, scope) {
            return function () {
                scope.$apply(function () {
                    deferred.resolve(reader.result);
                });
            };
        };

        var onError = function (reader, deferred, scope) {
            return function () {
                scope.$apply(function () {
                    deferred.reject(reader.result);
                });
            };
        };

        var onProgress = function (reader, scope) {
            return function (event) {
                scope.$broadcast("fileProgress",
                    {
                        total: event.total,
                        loaded: event.loaded
                    });
            };
        };

        var getReader = function (deferred, scope) {
            var reader = new FileReader();
            reader.onload = onLoad(reader, deferred, scope);
            reader.onerror = onError(reader, deferred, scope);
            reader.onprogress = onProgress(reader, scope);
            return reader;
        };

        var readAsDataURL = function (file, scope) {
            var deferred = $q.defer();

            var reader = getReader(deferred, scope);
            reader.readAsDataURL(file);

            return deferred.promise;
        };

        return {
            readAsDataUrl: readAsDataURL
        };
    }])

    .factory('$menu', function () {
        var sections = [];
        sections.push({
            id: 'package',
            name: '我要转运',
            type: 'toggle',
            pages: [{
                id: 'package-new',
                name: '原装闪运',
                type: 'link',
                url: '/Package?newPackage=true',
                icon: 'fa fa-group'
            }, {
                id: 'package-new-hold',
                name: '普通转运',
                url: '/Package?newPackage=true&holdPackage=true',
                type: 'link',
                icon: 'fa fa-map-marker'
            },
              {
                  id: 'package-manage',
                  name: '包裹管理',
                  url: '/Package',
                  type: 'link',
                  icon: 'fa fa-plus'
              }]
        });

        sections.push({
            id: 'package',
            name: '代购助手',
            type: 'toggle',
            pages: [{
                id: 'package-new',
                name: '商品列表',
                type: 'link',
                url: '/Goods',
                icon: 'fa fa-group'
            }, {
                id: 'package-new-hold',
                name: '代刷购物',
                url: '/Purchase',
                type: 'link',
                icon: 'fa fa-map-marker'
            }]
        });

        sections.push({
            id: 'account',
            name: '个人中心',
            type: 'link',
            url: '/Account'
        });

        sections.push({
            id: 'message',
            name: '站内留言',
            type: 'link',
            url: '/Message'
        });

        sections.push({
            id: 'logoff',
            name: '退出',
            type: 'link',
            url: '/Account/LogOff'
        });

        var self;

        return self = {
            sections: sections,
            openedSection: sections[0],
            toggleSelectSection: function (section) {
                self.openedSection = (self.openedSection === section ? null : section);
            },
            isSectionSelected: function (section) {
                return self.openedSection === section;
            },
        };
    });