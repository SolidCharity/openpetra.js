function ShowTree(html) {
    $("#trvAccountHierarchy").html(html);
}

// load account hierarchy from server
function LoadAccountHierarchy() {
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
}

function ExportAccountHierarchy() {
    $.ajax({
      type: "POST",
      url: "serverMFinance.asmx/TGLSetupWebConnector_ExportAccountHierarchyYmlGz",
      data: JSON.stringify({
            // TODO LedgerNumber not hard coded
            'ALedgerNumber': 43, 
            'AAccountHierarchyName': 'STANDARD', 
            }),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function(data, status, response) {
        result = JSON.parse(response.responseText);
        if (result['d'] != 0)
        {
            // alert("Successful result");
            console.debug(JXG.decompress(result['d']));
            var pom = document.createElement('a');
            pom.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(JXG.decompress(result['d'])));
            pom.setAttribute('download', "accounthierarchy.yml");
            pom.click();
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
}

jQuery(document).ready(function() {

    $('#btnExportAccountHierarchy').on('click', function (e) {
        ExportAccountHierarchy();
    });
    $('#btnImportAccountHierarchy').on('click', function (e) {
        alert("import");
        LoadAccountHierarchy();
    });
    
    LoadAccountHierarchy();
});
