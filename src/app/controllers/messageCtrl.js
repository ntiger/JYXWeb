/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('messageApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('messageCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
        $scope.$appUtil = $appUtil;
        $scope.messageCategories = ['系统问题', '清关问题', '包裹入库', '包裹出库', '充值问题', '其他问题'];
        $scope.messageStatusList = ['待回复', '已回复', '已解决', '全部'];
        $scope.defaultStatus = $scope.messageStatusList[0];
        $scope.messageStatus = $scope.messageStatusList[3];
        
        $scope.init = function () {
            $scope.getMessages();
        }

        $scope.getMessages = function () {
            var model = { status: $scope.messageStatus };
            $http.post('/Message/GetMessages', model).then(function (res) {
                $scope.messages = res.data;
            });
        }

        $scope.getMessage = function (index) {
            $http.get('/Message/GetMessage/' + $scope.messages[index].ID).then(function (res) {
                $scope.message = res.data;
            });
        }

        $scope.deleteMessage = function (ev, index) {
            $appUtil.appConfirm(ev, '', '删除以后将会无法找回这条留言, 确认要删除吗?', '确认', '取消', function () {
                $http.get('/Message/DeleteMessage/' + $scope.messages[index].ID).then(function (res) {
                    $scope.messages.splice(index, 1);
                });
            });
        }

        $scope.closeMessage = function () {
            $http.get('/Message/CloseMessage/' + $scope.message.ID).then(function (res) {
                $scope.message.Status = '已解决';
                angular.forEach($scope.messages, function (value) {
                    if (value.ID === $scope.message.ID) {
                        value.Status = '已解决'
                    }
                });
            });
        }

        $scope.deleteMessageContent = function (ev, index) {
            $appUtil.appConfirm(ev, '', '删除以后将会无法找回这条记录, 确认要删除吗?', '确认', '取消', function () {
                $http.get('/Message/DeleteMessageContent/' + $scope.message.MessageContents[index].ID).then(function (res) {
                    $scope.message.MessageContents.splice(index, 1);
                });
            });
        }

        $scope.postMessage = function (ev, comment) {
            var model = { message: $scope.message, messageStr: comment };
            $http.post('/Message/PostMessage', model).then(function (res) {
                $scope.comment = '';
                $scope.message = res.data;
                var hasMessage = false;
                angular.forEach($scope.messages, function (value) {
                    if (value.ID === $scope.message.ID) {
                        hasMessage = true;
                        value.Tracking = $scope.message.Tracking;
                        value.Timestamp = $scope.message.Timestamp;
                        value.Category = $scope.message.Category;
                    }
                });
                if (!hasMessage) {
                    $scope.messages.push($scope.message);
                }
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