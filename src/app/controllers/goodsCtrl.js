/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('goodsApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('goodsCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
        $scope.$appUtil = $appUtil;

        $scope.goods = [1, 2, 3, 4, 5, 6, 7, 8];
        $scope.categories = ['包','保健品','化妆品','衣服','鞋子']
        $scope.showUpload = function (ev) {
            $mdDialog.show({
                templateUrl: 'upload-dialog.html',
                scope: $scope,
                preserveScope: true,
                targetEvent: ev
            });
        };
    }]);