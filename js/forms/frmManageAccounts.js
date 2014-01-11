function ShowTree(html) {
    $("#trvAccountHierarchy").html(html);
}

jQuery(document).ready(function() {

    // load account hierarchy from server
    $.ajax({
      type: "POST",
      url: "serverMFinance.asmx/TGLSetupWebConnector_LoadAccountHierarchyHtmlCode",
      data: JSON.stringify({
            // TODO LedgerNumber not hard coded
            'ALedgerNumber': 43, 
            'AAccountHierarchyCode': 'STANDARD', 
            }),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function(data, status, response) {
        result = JSON.parse(response.responseText);
        if (result['d'] != 0)
        {
            // alert("Successful result");
            ShowTree(result['d']);
            EnableTree();
            // collapse some branches
            $('#acctASSETS span')[0].click();
            $('#acctLIABS span')[0].click();
            $('#acctINC span')[0].click();
            $('#acctEXP span')[0].click();
        }
        else
        {
            console.debug("something went wrong");
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
