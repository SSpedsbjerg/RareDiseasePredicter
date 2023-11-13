import './App.css';
import Header from './Header.js'
import Results from './Results.js'
import Input from './Input.js'
import { useState, useEffect } from "react";

function App() {
  const [response, setResponse] = useState(null);
  const [dataFromInput, setDataFromInput] = useState(null);

  function handleData  (data)  {
    setDataFromInput(data);
  };

  useEffect(() => {
    const postData = async () =>  {

      try {
      const response = await fetch('http://83.92.23.39:57693/GetSuggestion/', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(dataFromInput)
      });
      const data = await response.json();
      setResponse(data);
      } catch (error) {
        
      }
    };
  
    postData();
     }, [dataFromInput]);

  return (
    <div className="App">
        <Header />
      <div className="Content">
        <div className="Input">
        <Input handleData={handleData}/>
        </div>
        { response &&
        <div className="Results">
        <Results data={response} />
      </div> }
    </div>

    </div>
  );
}

export default App;
