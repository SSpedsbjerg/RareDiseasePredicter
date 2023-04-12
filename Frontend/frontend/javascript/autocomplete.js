function autocomplete(inp, arr) {
    
    var currentFocus;
    //execute a function when someone writes in the text field
    inp.addEventListener("input", function(e) {
        var a, b, i, count = 0, maxcount = 5,
        val = this.value;
        //close any already open lists of autocompleted values
        closeAllLists();
        if (!val) { return false;}

        currentFocus = -1;
        //create a DIV element that will contain the items (values)
        a = document.createElement("DIV");
        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items");
        //append the DIV element as a child of the autocomplete container
        this.parentNode.appendChild(a);
        for (i = 0; i < arr.length; i++) {
         

          if (arr[i].substr(0, val.length).toUpperCase() == val.toUpperCase() && count<5) {

            //create a DIV element for each matching element with matching description
            b = document.createElement("DIV");
            b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
            b.innerHTML += arr[i].substr(val.length);


            //insert a input field that will hold the current array item's value. Added regex for certain symbols
            b.innerHTML += "<input type='hidden' value='" + arr[i].replace(/(['\'])+/g,'&#39;')+ "'>";
                b.addEventListener("click", function(e) {
                //insert the value for the autocomplete text field
                inp.value = this.getElementsByTagName("input")[0].value;
                closeAllLists();
            });

            if (count<maxcount){
              a.appendChild(b);
              count++;
            }
          }

        }

        
    });


    //execute a function presses a key on the keyboard
    inp.addEventListener("keydown", function(e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
          //If the arrow DOWN key is pressed, increase the currentFocus variable
          currentFocus++;
          //Make current item visible
          addActive(x);
        } else if (e.keyCode == 38) { 
          //If the arrow UP key is pressed, decrease the currentFocus variable
          currentFocus--;
          addActive(x);
        } else if (e.keyCode == 13) {
          //If the ENTER key is pressed, prevent the form from being submitted
          e.preventDefault();
          if (currentFocus > -1) {
            //simulate click instead
            if (x) x[currentFocus].click();
          }
        }
    });


    function addActive(x) {
      //a function to classify an item as "active"
      if (!x) return false;
      removeActive(x);
      if (currentFocus >= x.length) currentFocus = 0;
      if (currentFocus < 0) currentFocus = (x.length - 1);
      x[currentFocus].classList.add("autocomplete-active");
    }

    function removeActive(x) {
      //a function to remove the "active" class from all autocomplete items
      for (var i = 0; i < x.length; i++) {
        x[i].classList.remove("autocomplete-active");
      }
    }

    function closeAllLists(element) {
      //close all autocomplete lists in the document, except the one passed as an argument
      var x = document.getElementsByClassName("autocomplete-items");
      for (var i = 0; i < x.length; i++) {
        if (element != x[i] && element != inp) {
        x[i].parentNode.removeChild(x[i]);
      }
    }
  }

  //execute a function when someone clicks in the document
  document.addEventListener("click", function (e) {
      closeAllLists(e.target);
  });
  }