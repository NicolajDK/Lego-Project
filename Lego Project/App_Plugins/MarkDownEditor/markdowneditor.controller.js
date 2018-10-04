angular.module("umbraco")
    .controller("My.MarkdownEditorController",
        function ($scope, assetsService, dialogService, imageHelper) {
            
            if ($scope.model.value === null || $scope.model.value === "") {
                $scope.model.value = $scope.model.config.defaultValue;
            }

            assetsService.load([
                "~/App_Plugins/MarkDownEditor/lib/markdown.converter.js",
                "~/App_Plugins/MarkDownEditor/lib/markdown.sanitizer.js",
                "~/App_Plugins/MarkDownEditor/lib/markdown.editor.js"
            ]).then(function() {
            	var converter2 = new Markdown.Converter();
            	var editor2 = new Markdown.Editor(converter2, "-" + $scope.model.alias);
            	editor2.run();

                editor2.hooks.set("insertImageDialog",
                    function (callback) {

                        dialogService.mediaPicker({
                            callback: function(data) {
                                $(data.selection).each(function(i, item) {
                                	$log.log(item);

                                    var imagePropVal =
                                        imageHelper.getImagePropertyValue({ imageModel: item, scope: $scope });
                                    callback(imagePropVal);
                                });
                            }
                        });

                        return true;
                    });

            });
            assetsService.loadCss("~/App_Plugins/MarkDownEditor/lib/docs.css");
        });