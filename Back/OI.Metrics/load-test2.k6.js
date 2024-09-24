import http from 'k6/http';
import {sleep} from 'k6'

export const options = {
  vus: 10,
  duration: '5s',
};

const url = 'http://localhost:8000/Jobs?sigrConnId=fakeId';

export default function () {
  // http.post('http://localhost:8000/Jobs?sigrConnId=qweqweqwe', {
  //   "stringToConvert": "Hello World!"
  // })
  // sleep(1);

  //const params = new HttpParams().set('sigrConnId', "fakeId");
  const data = {'stringToConvert': "Hi there"};
  // http.post('http://localhost:8000/Jobs?sigrConnId=fakeId', 
  //   payload
  // )

  let res = http.post(url, JSON.stringify(data), {
    headers: { 'Content-Type': 'application/json' },
  });
}