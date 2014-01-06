jQuery(document).ready(function() {
    $("#btnSearch").click(function(e) {
        e.preventDefault();
        $.ajax({
          type: "POST",
          url: "/serverMPartner.asmx/TSimplePartnerFindWebConnector_FindPartners",
          data: JSON.stringify({
                'AFamilyNameOrOrganisation': $("#partnername").val(), 
                'AFirstName': $("#firstname").val(),
                'APartnerClass': '*',
                'ACity': $("#city").val(),
                }),
          contentType: "application/json; charset=utf-8",
          dataType: "json",
          success: function(data, status, response) {
            result = JSON.parse(response.responseText);
            console.debug(JSON.stringify(response.responseText));
            if (result['d'] != 0)
            {
                // alert("Successful result");
                SearchResult = JSON.parse(result['d'])["SearchResult"];
                for (index = 0; index < SearchResult.length; ++index) {
                    alert(SearchResult[index]["p_partner_short_name_c"]);
                }
            }
            else
            {
                alert("something went wrong");
            }
          },
          error: function(response, status, error) {
            console.debug(error);
            console.debug(JSON.stringify(response.responseJSON));
            alert("Server error, please try again later");
          },
          fail: function(msg) {
            console.debug(msg);
            alert("Server failure, please try again later");
          }
        });      
    });
});
