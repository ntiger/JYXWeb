/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('accountApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('accountCtrl', function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
    $scope.$appUtil = $appUtil;

    $scope.saveName = function () {
        $http.post('/Manage/SaveName', { firstName: $scope.firstName, lastName: $scope.lastName }).then(function (res) { });
    }

    $scope.getUsers = function () {
        if ($scope.isAdmin) {
            $http.get('/Manage/GetUsers').then(function (res) {
                $scope.users = res.data;
            });
        }
    }

    

    

    $scope.query = {
        order: 'UserCode',
        limit: 10,
        page: 1
    };
});