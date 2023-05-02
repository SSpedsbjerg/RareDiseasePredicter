import Table from 'react-bootstrap/Table';

function Results({ data }) {
    return(
      <div>
        {data && (
        <Table striped bordered hover>
        <thead>
          <tr>
            <th>Sygdom</th>
            <th>Sandsynlighed</th>
          </tr>
        </thead>
        <tbody>
        {data.map(disease => (
            <tr key={disease.id} >
              <td>{disease.name}</td>
              <td>{disease.chance}</td>
            </tr>
          ))}
        </tbody>
      </Table>
      )}
      </div>
    );
}

export default Results;