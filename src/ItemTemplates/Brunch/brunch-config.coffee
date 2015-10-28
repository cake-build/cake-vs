exports.config =
  # See http://brunch.io/ for documentation.
  paths:
    public: 'wwwroot'
  files:
    javascripts:
      joinTo:
        'js/app.js' : /^app/
        'js/libs.js': /(vendor|bower_components)/
    stylesheets:
      joinTo:
        '/css/app.css' : /^app/
        '/css/libs.css' : /(vendor|bower_components)/
  modules:
    wrapper: false