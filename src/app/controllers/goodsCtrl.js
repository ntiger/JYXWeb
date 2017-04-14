/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('goodsApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('goodsCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$fileReader', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $fileReader, $menu) {
        $scope.$appUtil = $appUtil;
        $scope.goods = [];
        $scope.categories = ['所有商品', '包', '保健品', '化妆品', '衣服', '鞋子', '食品', '日用品'];
        $scope.inputCategories = ['包', '保健品', '化妆品', '衣服', '鞋子', '食品', '日用品'];
        $scope.category = '所有商品';
        $scope.statusList = ['已提交', '已结单', '已取消', '全部'];
        $scope.defaultStatus = $scope.statusList[0];
        $scope.orderStatus = $scope.statusList[3];

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
            $scope.quantity = 1;
            $scope.getGoodsEntry(goodsEntry, function () {
                $scope.selectedItem = $scope.goodsEntry.GoodsItems[0];
                $scope.goodsEntry.ColorSizes[0].Selected = true;
                $scope.goodsEntry.ColorSizes[0].Sizes[0].Selected = true;
                $scope.detailImage = $scope.goodsEntry.GoodsItems[0].GoodsImages[0].Image;
                $mdDialog.show({
                    controller: DialogController,
                    templateUrl: 'detail-dialog.html',
                    scope: $scope,
                    preserveScope: true,
                    targetEvent: ev
                });
            })
        };
        
        $scope.getGoodsEntry = function (goodsEntry, callback) {
            $http.post('/Goods/GetGoodsEntry/' + goodsEntry.ID).then(function (res) {
                $scope.goodsEntry = res.data;
                if (callback) { callback();}
            });
        }

        $scope.showOrders = function (ev) {
            $scope.getOrders();
            $mdDialog.show({
                controller: DialogController,
                templateUrl: 'orders-dialog.html',
                scope: $scope,
                preserveScope: true,
                targetEvent: ev
            });
        }

        $scope.getOrders = function () {
            var model = {};
            model.status = $scope.orderStatus;
            $http.post('/Goods/GetOrders', model).then(function (res) {
                $scope.orders = res.data;
            });
        }
        
        $scope.getOrder = function (index, callback) {
            $http.post('/Goods/GetOrder/' + $scope.orders[index].ID).then(function (res) {
                $scope.order = res.data;
                if (callback) { callback($scope.order); }
            });
        }

        $scope.deleteOrder = function (index) {
            $appUtil.appConfirm(ev, '', '删除以后将会无法找回这条订单, 确认要删除吗?', '确认', '取消', function () {
                $http.get('/Goods/DeleteOrder/' + $scope.orders[index].ID).then(function (res) {
                    $scope.orders.splice(index, 1);
                });
            });
        }

        $scope.confirmOrder = function (ev, index) {
            $scope.getOrder(index, function (order) {
                order.Status = $scope.statusList[1];
                $scope.order = order;
                $scope.updateOrder();
            });
        }

        $scope.cancelOrder = function (ev, index) {
            $scope.getOrder(index, function (order) {
                order.Status = $scope.statusList[2];
                $scope.order = order;
                $scope.updateOrder();
            });
        }

        function DialogController($scope, $mdDialog) {
            $scope.close = function () {
                $mdDialog.cancel();
            };

            $scope.addImage = function () {
                $fileReader.readAsDataUrl($scope.childFileSelectScope.files[$scope.childFileSelectScope.files.length - 1], $scope.childFileSelectScope).then(function (result) {
                    if (typeof $scope.childFileSelectScope.currentItem.GoodsImages === 'undefined') {
                        $scope.childFileSelectScope.currentItem.GoodsImages = [];
                    }
                    $scope.childFileSelectScope.currentItem.GoodsImages.push({ Image: result });
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

            $scope.selectColor = function (color) {
                $scope.selectSize(color);
            }

            $scope.selectSize = function (color, size) {
                angular.forEach($scope.goodsEntry.ColorSizes, function (value) {
                    value.Selected = false;
                    if (value.Color === color) {
                        value.Selected = true;
                        angular.forEach(value.Sizes, function (value) {
                            value.Selected = false;
                            if (value.Size === size) {
                                value.Selected = true;
                            }
                        });
                        if (typeof size === 'undefined') {
                            value.Sizes[0].Selected = true;
                            size = value.Sizes[0].Size;
                        }
                    }
                });
                angular.forEach($scope.goodsEntry.GoodsItems, function (value) {
                    if (value.Color === color && value.Size === size) {
                        $scope.selectedItem = value;
                        $scope.detailImage = value.GoodsImages[0].Image;
                    }
                });
            }

            $scope.selectGoodsItem = function (item) {
                $scope.selectedItem = item;
                $scope.selectSize(item.Color, item.Size)
            }

            $scope.selectImage = function (image) {
                $scope.detailImage = image;
            }

            $scope.purchase = function (ev) {
                if ($scope.quantity > $scope.selectedItem.Quantity) {
                    $appUtil.appAlert(ev, '', '购买数量不能超过库存量，请重新输入');
                    return;
                }
                $appUtil.getBalance(function (balance) {
                    if (!$scope.isAdmin && balance - $scope.quantity * $scope.selectedItem.Price < 0) {
                        $appUtil.appAlert(ev, '', '您的余额不足，请到个人中心充值');
                        return;
                    }

                    if (confirm('确认购买吗?')) {
                        var goodsOrder = {
                            Quantity: $scope.quantity,
                            GoodsItem: $scope.selectedItem,
                            Status: $scope.defaultStatus,
                        };
                        $http.post('/Goods/UpdateOrder/', { order: goodsOrder }).then(function (res) {
                            $appUtil.appConfirm(ev, '', '代购订单已生成, 请问您要自动生成包裹运单吗?', '生成包裹', '取消', function () {
                                $http.get('/Goods/CreatePackage/' + res.data).then(function (res) {
                                    $appUtil.appAlert(ev, '', '包裹运单已生成，请到包裹管理页添加收件人/发件人信息，并且进行需要的分箱/合箱操作');
                                });
                            });
                            $scope.search();
                        });
                    }
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
        $menu.toggleSelectSection($menu.sections[1]);
    }]);