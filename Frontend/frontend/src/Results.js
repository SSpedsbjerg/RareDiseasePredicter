import Table from 'react-bootstrap/Table';

function Results({data}) {
  if (!Array.isArray(data)) {
    console.log(data);
    return <div>No data available</div>;
  }

    return(
      <div>
        {data && (
        <Table striped bordered hover>
        <thead>
          <tr>
            <th>Sygdom</th>
            <th>Sandsynlighed</th>
            <th>Link</th>
          </tr>
        </thead>
        <tbody>
        {data.map(disease => (
            <tr key={disease.ID} >
              <td>{disease.Name}</td>
              <td>{disease.Weight}</td>
              <td><a href={disease.Href} rel="noreferrer">Info</a></td>
            </tr>
          ))}
        </tbody>
      </Table>
      )}
      </div>
    );
}

export default Results;