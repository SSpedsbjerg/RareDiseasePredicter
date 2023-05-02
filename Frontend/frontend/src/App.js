import './App.css';
import Header from './Header.js'
import Results from './Results.js'
import Input from './Input.js'

function App() {
  return (
    <div className="App">
        <Header />
      <div className="Content">
        <div className="Input">
        <Input />
        </div>
        <div className="Results">
        <Results />
      </div>
    </div>

    </div>
  );
}

export default App;
