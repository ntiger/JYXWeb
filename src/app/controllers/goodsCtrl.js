/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('goodsApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('goodsCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$fileReader', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $fileReader, $menu) {
        $scope.$appUtil = $appUtil;
        $scope.goods = [];
        $scope.categories = ['包', '保健品', '化妆品', '衣服', '鞋子', '食品', '日用品']
        $scope.category = $scope.categories[0];

        $scope.init = function () {
            $scope.search();
        }

        $scope.search = function () {
            var model = {};
            model.category = $scope.category;
            $http.post('/Goods/Search', model).then(function (res) {
                $scope.goods = res.data;
            });
        }

        $scope.showUpload = function (ev) {
            $mdDialog.show({
                controller: DialogController,
                templateUrl: 'upload-dialog.html',
                scope: $scope,
                preserveScope: true,
                targetEvent: ev
            });
        };

        $scope.showDetail = function (ev, goodsEntry) {
            $scope.goodsEntry = goodsEntry;
            $scope.goodsEntry.ColorSizes = {};
            angular.forEach($scope.goodsEntry.GoodsItems, function (value) {
                if (value.Color) {
                    if (typeof $scope.goodsEntry.ColorSizes[value.Color] === 'undefined') {
                        $scope.goodsEntry.ColorSizes[value.Color] = [];
                    }
                    $scope.goodsEntry.ColorSizes[value.Color].push(value);
                }
            });
            $scope.selectedItem = goodsEntry.GoodsItems[0];
            $scope.detailImage = goodsEntry.GoodsItems[0].GoodsImages[0].Image;
            $mdDialog.show({
                controller: DialogController,
                templateUrl: 'detail-dialog.html',
                scope: $scope,
                preserveScope: true,
                targetEvent: ev
            });
        };
        

        function DialogController($scope, $mdDialog) {
            $scope.close = function () {
                $mdDialog.cancel();
            };

            $scope.addImage = function (scope) {
                $fileReader.readAsDataUrl(scope.files[scope.files.length - 1], $scope).then(function (result) {
                    if (typeof scope.currentItem.GoodsImages === 'undefined') {
                        scope.currentItem.GoodsImages = [];
                    }
                    scope.currentItem.GoodsImages.push({ Image: result });
                });
            };

            $scope.updateGoodsEntry = function () {
                $scope.showLoading = true;
                $http.post('/Goods/Update', { goods: $scope.goodsEntry }).then(function (res) {
                    $scope.showLoading = false;
                    $mdDialog.cancel();
                    $scope.search();
                });
            }
        }

        // SideNav
        var vm = this;
        vm.$menu = $menu;
        vm.isOpen = function (section) {
            return $menu.isSectionSelected(section);
        };
        vm.toggleOpen = function (section) {
            $menu.toggleSelectSection(section);
        }
        console.log($menu)
    }]);