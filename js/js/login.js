jQuery(document).ready(function() {
    $("#btnLogin").click(function(e) {
        e.preventDefault();
        $.ajax({
          type: "POST",
          url: "/server.asmx/Login",
          data: JSON.stringify({'username': $("#txtEmail").val(), 'password': $("#txtPassword").val()}),
          contentType: "application/json; charset=utf-8",
          dataType: "json",
          success: function(data, status, response) {
            result = JSON.parse(response.responseText);
            if (result['d'] == true)
            {
                // alert("Successful logged in");
                window.location = "Main.aspx";
            }
            else
            {
                alert("Wrong username or password, please try again");
                // $('#container').effect('shake', { times:3 }, 200);
            }
          },
          error: function(response, status, error) {
            alert("Server error, please try again later");
          },
          fail: function(msg) {
            alert("Server failure, please try again later");
          }
        });      
    });
});
