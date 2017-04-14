angular.module('appDirective', [])
    .directive('menuLink', function () {
        return {
            scope: {
                section: '='
            },
            templateUrl: '/src/app/templates/menu-link.tmpl.html',
            link: function ($scope, $element) {
                var controller = $element.parent().controller();

                $scope.focusSection = function () {
                    // set flag to be used later when
                    // $locationChangeSuccess calls openPage()
                    controller.autoFocusContent = true;
                };
            }
        };
    })

    .directive('menuToggle', ['$timeout', function ($timeout) {
        return {
            scope: {
                section: '='
            },
            templateUrl: '/src/app/templates/menu-toggle.tmpl.html',
            link: function ($scope, $element) {
                var controller = $element.parent().controller();
                $scope.isOpen = function () {
                    return controller.isOpen($scope.section);
                };
                $scope.toggle = function () {
                    controller.toggleOpen($scope.section);
                };
            }
        };
    }])

    .directive('docsScrollClass', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attr) {

                var scrollParent = element.parent();
                var isScrolling = false;

                // Initial update of the state.
                updateState();

                // Register a scroll listener, which updates the state.
                scrollParent.on('scroll', updateState);

                function updateState() {
                    var newState = scrollParent[0].scrollTop !== 0;

                    if (newState !== isScrolling) {
                        element.toggleClass(attr.docsScrollClass, newState);
                    }

                    isScrolling = newState;
                }
            }
        }
    })

    .directive('packageTracking', ['$http', function ($http) {
        return {
            scope: {

            },
            templateUrl: '/src/app/templates/package-tracking.tmpl.html',
            link: function ($scope, $element) {
                $scope.track = function (code) {
                    $http.get('/Package/Tracking/' + code).then(function (res) {
                        $scope.trackings = res.data;
                        $('#trackingModal').modal('show');
                    });
                };
            }
        };
    }])

    .directive("ngFileSelect", ['$parse', function ($parse) {
        return {
            link: function ($scope, el, attrs) {
                var expressionHandler = $parse(attrs.ngFileSelect);
                el.bind("change", function (e) {
                    $scope.files = (e.srcElement || e.target).files;
                    $scope.$parent.childFileSelectScope = $scope;
                    console.log($scope)
                    expressionHandler($scope);
                    e.target.value = '';
                });
            }
        }
    }])
;