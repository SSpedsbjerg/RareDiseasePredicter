//Create new row with symptom and region in table "symptomtable"
function appen() {
    var table = document.getElementById("symptomtable");
    var str1 = document.getElementById("region");
    var str2 = document.getElementById("symptom");


    var row = table.insertRow(1);
    var cell1 = row.insertCell(0);
    var cell2 = row.insertCell(1);
    var cell3 = row.insertCell(2);
    cell3.innerHTML = '<img src="images/cross.svg" width="20" height="20" onclick="deleteRowFunction(this)" />';
    cell1.innerHTML = str1.value;
    cell2.innerHTML = str2.value;
    str2.value = "";
    str1.value = "";
  }
 //Get rows' parent and delete the child (row) as parent
  function deleteRowFunction(row) {
    var parent=row.parentNode.parentNode;
        parent.parentNode.removeChild(parent);
   }