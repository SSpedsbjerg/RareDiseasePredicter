import React from 'react';
import './Input.css'
import { BodyComponent } from "reactjs-human-body";
import Table from 'react-bootstrap/Table';
import { useEffect, useState } from "react";
import Button from 'react-bootstrap/Button'
import { Autocomplete, TextField } from '@mui/material';


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

    const [data, setData] = useState({});
    const [value, setValue] = useState(null);
    const [response, setResponse] = useState(null);
  
    
  

    useEffect(() => {
      fetchMovies();
      
    }, []);

    function fetchMovies() {
      fetch(
        `http://localhost:57693/symptoms`
      )
        .then((response) => response.json())
        .then((data) => {
          setData(data)
        }
        );
      }

    const showBodyPart = (e) => {
      const trueKeys = Object.entries(bodyState)
      .filter(([key, obj]) => obj.selected === true)
      .map(([key]) => key);
      };


      const getRegions = () => {
       const regions = Object.entries(bodyState)
      .filter(([key, obj]) => obj.selected === true)
      .map(([key]) => key);

      
      const updatedRegions = regions.map((bodypart) => {
        switch (bodypart) {
          case "head":
            return "Hoved";
          case "left_shoulder":
            return "Venstre Skulder";
          case "right_shoulder":
            return "Højre skulder";
          case "left_arm":
            return "Venstre arm";
          case "right_arm":
            return "Højre arm";
          case "chest":
            return "Bryst";
          case "stomach":
            return "Mave";
          case "left_leg":
            return "Venstre ben";
          case "right_leg":
            return "Højre ben";
          case "left_hand":
            return "Venstre hånd";
          case "right_hand":
            return "Højre hånd";
          case "left_foot":
            return "Venstre fod";
          case "right_foot":
            return "Højre fod";
          default:
            return bodypart;
        }
      });
      
      return updatedRegions.join(", ");
      }

     const addSymptom = (event) => {
        event.preventDefault();
        const newRowData = {
          symptom: value.Name,
          region: getRegions(),
        }
        setTableData([...tableData, newRowData ]);
    }

    const removeSymptom = (index) => {
      const newData = [...tableData];
      newData.splice(index, 1);
      setTableData(newData);
    };

    const postData = async () =>  {
      const stringArray = tableData.map(data => data.symptom);
      const response = await fetch('https://83.92.23.39:57693/GetSuggestion/', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(stringArray)
      });
      const data = await response.json();
      setResponse(data);
        console.log(data);
    }


    return(
        <div>
            <div className="Human">
            <BodyComponent
          partsInput={bodyState}
          onClick={(e) => showBodyPart(e)}
        />
        <div>
          <Button 
          variant="success"
          type="submit"
          onClick={postData}>
            Find sygdom
          </Button>
        </div>
            </div>
            <form onSubmit={addSymptom}>
              <div className="InputArea">
                <div>
                Symptom 
                </div>
                  
                  <Autocomplete
                  className="Autocomplete"
                  options={data}
                  getOptionLabel={(data) => data.Name}
                  value={value}
                  onChange={(event, newValue) => {
                    setValue(newValue);
                  }}
                  sx={{ width: 300 }}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Symptom"
                      variant="outlined"
                    
                    />
                  )}
                />

              </div>
              <div>
                <Button 
                variant="success"
                type="submit"
                className="AddButton"
                > Tilføj</Button>
              </div><br/><br/>
            </form>
            <div className="TableView">
            <Table className="SymptomTable" striped >
        <thead>
          <tr>
            <th>Region</th>
            <th>Symptom</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
        {tableData.map((rowData, index) => (
            <tr key={index} >
              <td>{rowData.region}</td>
              <td>{rowData.symptom}</td>
              <td>
                <Button className='removeButton' variant='danger' onClick={() => removeSymptom(index)}>Remove</Button>
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