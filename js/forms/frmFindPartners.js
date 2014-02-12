function Init() {
    $("#btnSearch").click(function(e) {
        e.preventDefault();
        $.ajax({
          type: "POST",
          url: "serverMPartner.asmx/TSimplePartnerFindWebConnector_FindPartners",
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
            // console.debug(JSON.stringify(response.responseText));
            if (result['d'] != 0)
            {
                // alert("Successful result");
                SearchResult = JSON.parse(result['d'])["SearchResult"];
                ParsedSearchResult = new Array();
                
                for (index = 0; index < SearchResult.length; ++index) {
                    // alert(SearchResult[index]["p_partner_short_name_c"]);
                    ParsedSearchResult.push(new Array(
                        SearchResult[index]["p_partner_class_c"],
                        SearchResult[index]["p_partner_short_name_c"],
                        SearchResult[index]["p_city_c"],
                        SearchResult[index]["p_street_name_c"],
                        SearchResult[index]["p_partner_key_n"],
                        SearchResult[index]["p_status_code_c"],
                        SearchResult[index]["p_location_key_i"]
                        ));
                }

                // see http://datatables.net/release-datatables/examples/data_sources/js_array.html
                $('#result').dataTable( {
                        "aaData": ParsedSearchResult,
                        "bDestroy": true
                    } );

                // see http://datatables.net/release-datatables/examples/api/select_single_row.html
                $("#result tbody tr").click( function( e ) {
                    if ( $(this).hasClass('row_selected') ) {
                        $(this).removeClass('row_selected');
                    }
                    else {
                        $('#result tr.row_selected').removeClass('row_selected');
                        $(this).addClass('row_selected');
                    }
                });

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

    $("#btnClear").click(function(e) {
        e.preventDefault();
        $('#formPartnerFind')[0].reset();
    });

    // Add a click handler for the partner edit
    $("#btnEditPartner").click( function(e) {
        e.preventDefault();
        var anSelected = $('#result tr.row_selected');;
        if ( anSelected.length !== 0 ) {
            console.debug(anSelected);
            // TODO open partner edit oTable.fnDeleteRow( anSelected[0] );
            window.parent.OpenTab("frmPartnerEdit", "Edit Partner");
        }
    });

    $.ajax({
      type: "POST",
      url: "serverMCommon.asmx/TCommonCacheableWebConnector_GetCacheableTable",
      data: JSON.stringify({
            'ACacheableTable': 'CountryList', 
            'AHashCode': '',
            }),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function(data, status, response) {
        result = JSON.parse(response.responseText);
        //console.debug(JSON.stringify(response.responseText));
        //console.debug(JSON.stringify(result));
        if (result['d'] != 0)
        {
            //console.debug(JSON.stringify(result['d']));
            //console.debug(JSON.stringify(JSON.parse(result['d'])));
            
            SearchResult = JSON.parse(result['d'])[1];
            
            ParsedSearchResult = new Array();
            
            options = "<option></option>";
            for (index = 0; index < SearchResult.length; ++index) {
                options += "<option value='" + SearchResult[index]["p_country_code_c"] + "'>" + 
                    SearchResult[index]["p_country_name_c"] + "</option>";
            }
            $("#country").html(options);
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
}
