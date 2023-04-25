import React from 'react';
import './Input.css'
import { BodyComponent } from "reactjs-human-body";
import Table from 'react-bootstrap/Table';
import { useEffect, useState } from "react";
import Button from 'react-bootstrap/Button'

function Input() {
    const [bodyState, setBodyState] = useState({
        head: {
          show: true,
          selected: false
        },
        left_shoulder: {
          show: true,
          selected: false
        },
        right_shoulder: {
          show: true,
          selected: false
        },
        left_arm: {
          show: true,
          selected: false
        },
        right_arm: {
          show: true,
          selected: false
        },
        chest: {
          show: true,
          selected: false
        },
        stomach: {
          show: true,
          selected: false
        },
        left_leg: {
          show: true,
          selected: false
        },
        right_leg: {
          show: true,
          selected: false
        },
        left_hand: {
          show: true,
          selected: false
        },
        right_hand: {
          show: true,
          selected: false
        },
        left_foot: {
          show: true,
          selected: false
        },
        right_foot: {
          show: true,
          selected: false
        }
      });



      const [tableData, setTableData] = useState([]);

    const showBodyPart = (e) => {
      const trueKeys = Object.entries(bodyState)
      .filter(([key, obj]) => obj.selected === true)
      .map(([key]) => key);
      

      console.log(trueKeys);
      console.log(e);
      
      };


      const getRegions = () => {
       const regions = Object.entries(bodyState)
      .filter(([key, obj]) => obj.selected === true)
      .map(([key]) => key);
      console.log(regions)
          //TODO Formater hvordan teksten kommer tilbage så det ikke ligner lort og er på dansk og tilføj auto complete med fetch request fra backend som auto complete liste :D
      return regions;
      }

     const addSymptom = (event) => {
        event.preventDefault();
        const newRowData = {
          symptom: event.target.elements.newRowData.value,
          region: getRegions(),
        }
        setTableData([...tableData, newRowData ]);
        
    }

    const removeSymptom = (index) => {
      const newData = [...tableData];
      newData.splice(index, 1);
      setTableData(newData);
    };
    return(
        <div>
            <div className="Human">
            <BodyComponent
          partsInput={bodyState}
          onClick={(e) => showBodyPart(e)}
        />
            </div>
            <form onSubmit={addSymptom}>
              <div className="InputArea">
                  Symptom <br/>
                  <input type="text" id="newRowData" placeholder="Symptom"></input> 
              </div>
              <br/><br/><br/>
              <div>
                <Button 
                variant="success"
                type="submit"
                className="AddButton"
                > Tilføj</Button>
              </div><br/><br/>
            </form>
            <div className="TableView">
            <Table className="SymptomTable">
        <thead>
          <tr>
            <th>Region</th>
            <th>Symptom</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
        {tableData.map((rowData, index) => (
            <tr key={index}>
              <td>{rowData.region}</td>
              <td>{rowData.symptom}</td>
              <td>
                <button onClick={() => removeSymptom(index)}>Remove</button>
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
            </div>
        </div>


    );
}

export default Input;