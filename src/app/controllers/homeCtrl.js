/* Controller */
if (typeof angularApp === 'undefined') {
    angularApp = angular.module('homeApp', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'md.data.table',
        'appFltr', 'appFctry', 'appSrvc', 'appDirective']);
}
angularApp.controller('homeCtrl', ['$scope', '$http', '$filter', '$log', '$timeout', '$appUtil', '$mdDialog', '$menu',
    function ($scope, $http, $filter, $log, $timeout, $appUtil, $mdDialog, $menu) {
        $scope.$appUtil = $appUtil;


    }]);