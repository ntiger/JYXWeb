/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('purchaseApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('purchaseCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$fileReader', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $fileReader, $menu) {
        $scope.$appUtil = $appUtil;
        $scope.statusList = ['已提交', '已购买', '已取消', '全部'];
        $scope.defaultStatus = $scope.statusList[0];
        $scope.orderStatus = $scope.statusList[3];

        $scope.purchases = [];

        $scope.init = function () {
            $scope.getOrders();
        }

        $scope.getOrders = function () {
            var model = {};
            model.status = $scope.orderStatus;
            $http.post('/Purchase/GetOrders', model).then(function (res) {
                $scope.orders = res.data;
            });
        }

        $scope.getOrder = function (index, callback) {
            $http.post('/Purchase/GetOrder/' + $scope.orders[index].ID).then(function (res) {
                $scope.order = res.data;
                if (callback) { calback($scope.order );}
            });
        }

        $scope.deleteOrder = function (index) {
            $appUtil.appConfirm(ev, '', '删除以后将会无法找回这条订单, 确认要删除吗?', '确认', '取消', function () {
                $http.get('/Purchase/DeleteOrder/' + $scope.orders[index].ID).then(function (res) {
                    $scope.orders.splice(index, 1);
                });
            });
        }
        
        $scope.confirmOrder = function (ev, index) {
            $scope.getOrder(index, function (order) {
                order.Status = statusList[1];
                $scope.updateOrder(order);
            });
        }

        $scope.cancelOrder = function (ev, index) {
            $scope.getOrder(index, function (order) {
                order.Status = statusList[2];
                $scope.updateOrder(order);
            });
        }

        $scope.deleteOrder = function (ev, index) {
            $appUtil.appConfirm(ev, '', '删除以后将会无法找回这条记录, 确认要删除吗?', '确认', '取消', function () {
                $http.get('/Purchase/DeleteOrder/' + $scope.orders[index].ID).then(function (res) {
                    $scope.getOrders();
                });
            });
        }


        $scope.showPurchaseOrder = function (ev, index) {
            if (typeof index !== 'undefined') {
                $scope.getOrder(index);
            }
            $mdDialog.show({
                controller: DialogController,
                templateUrl: 'purchase-order-dialog.html',
                scope: $scope,
                preserveScope: true,
                targetEvent: ev
            });
        };

        function DialogController($scope, $mdDialog, $appUtil) {
            $scope.close = function () {
                $mdDialog.cancel();
            };

            $scope.addImage = function () {
                $fileReader.readAsDataUrl($scope.files[$scope.files.length - 1], $scope).then(function (result) {
                    if (typeof $scope.order.PurchaseOrderImages === 'undefined') {
                        $scope.order.PurchaseOrderImages = [];
                    }
                    $scope.order.PurchaseOrderImages.push({ Image: result });
                });
            };

            $scope.updateOrder = function (ev) {
                $scope.showLoading = true;
                $http.post('/Purchase/UpdateOrder', { order: $scope.order }).then(function (res) {
                    $scope.showLoading = false;
                    $mdDialog.cancel();
                    $scope.getOrders();
                    $appUtil.appConfirm(ev, '', '代刷订单已生成, 请问您要自动生成或更新包裹吗?', '生成包裹', '取消', function () {
                        $http.get('/Purchase/CreatePackage/' + $scope.order.ID).then(function (res) {
                            $appUtil.appAlert(ev, '', '包裹运单已生成，请到包裹管理页添加收件人/发件人信息，并且进行需要的分箱/合箱操作');
                        });
                    });
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
    }]);