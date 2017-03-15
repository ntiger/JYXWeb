/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('messageApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('messageCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
        $scope.$appUtil = $appUtil;
        $scope.messageCategories = ['系统问题', '清关问题', '包裹入库', '包裹出库', '充值问题', '其他问题'];
        $scope.messageStatus = '待回复';

        $scope.init = function () {
            $scope.getMessages();
        }

        $scope.getMessages = function () {
            $http.get('/Message/GetMessages').then(function (res) {
                $scope.messages = res.data;
            });
        }

        $scope.getMessage = function (index) {
            $http.get('/Message/GetMessage/' + $scope.messages[index].ID).then(function (res) {
                $scope.message = res.data;
            });
        }

        $scope.delete = function (index) {
            if (confirm('确认要删除这条留言吗?')) {
                $http.get('/Message/DeleteMessage/' + $scope.messages[index].ID).then(function (res) {
                    $scope.messages.splice(index, 1);
                });
            }
        }

        $scope.deleteMessage = function (index) {
            if (confirm('确认要删除这条记录吗?')) {
                $http.get('/Message/DeleteMessage/' + $scope.message.MessageContents[index].ID).then(function (res) {
                    $scope.message.messageContents.splice(index, 1);
                });
            }
        }

        $scope.postMessage = function (comment) {
            var model = { message: $scope.message, messageStr: comment };
            $http.post('/Message/PostMessage', model).then(function (res) {
                $scope.comment = '';
                $scope.message = res.data;
            });
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