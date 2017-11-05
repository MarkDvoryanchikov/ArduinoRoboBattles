/*   
 *   Приемник 
*/

#include <SPI.h>
#include "nRF24L01.h"
#include "RF24.h"


byte recieved_data[5];
const int fight = 5; // реле
                     // левый мотор
const int LPWM = 7;
const int RPWM = 5;
const int EN = 6;
                     // правый мотор
const int LPWM2 = 4;
const int RPWM2 = 2;
const int EN2 = 3;

//RF24 radio(9,10); // "создать" модуль на пинах 9 и 10 Для Уно
RF24 radio(9,53); // для Меги

byte address[][6] = {"1Node","2Node","3Node","4Node","5Node","6Node"};  //возможные номера труб

void setup(){
  Serial.begin(9600); //открываем порт для связи с ПК
  radio.begin(); //активировать модуль
  radio.setAutoAck(1);         //режим подтверждения приёма, 1 вкл 0 выкл
  radio.setRetries(0,15);     //(время между попыткой достучаться, число попыток)
  radio.enableAckPayload();    //разрешить отсылку данных в ответ на входящий сигнал
  radio.setPayloadSize(32);     //размер пакета, в байтах
  
  radio.openReadingPipe(1,address[0]);      //хотим слушать трубу 0
  radio.setChannel(0x88);  //выбираем канал (в котором нет шумов!)
  
  radio.setPALevel (RF24_PA_MAX); //уровень мощности передатчика. На выбор RF24_PA_MIN, RF24_PA_LOW, RF24_PA_HIGH, RF24_PA_MAX
  radio.setDataRate (RF24_250KBPS); //скорость обмена. На выбор RF24_2MBPS, RF24_1MBPS, RF24_250KBPS
  //должна быть одинакова на приёмнике и передатчике!
  //при самой низкой скорости имеем самую высокую чувствительность и дальность!!
  
  radio.powerUp(); //начать работу
  radio.startListening();  //начинаем слушать эфир, мы приёмный модуль
  
  pinMode(LPWM, OUTPUT);
  pinMode(RPWM, OUTPUT);
  pinMode(EN, OUTPUT);
  pinMode(LPWM2, OUTPUT);
  pinMode(RPWM2, OUTPUT);
  pinMode(EN2, OUTPUT);
  digitalWrite(EN, HIGH);
  digitalWrite(EN2, HIGH);
  analogWrite(LPWM, 0);
  analogWrite(RPWM, 0);
  analogWrite(LPWM2, 0);
  analogWrite(RPWM2, 0);
}

int a, b, c, d, e;
int i = 0;
void loop() {
  if ( radio.available() ){    // слушаем эфир со всех труб
    radio.read( &recieved_data, sizeof(recieved_data) );         // чиатем входящий сигнал

    i = 0;
    // при потерях игнорируем нули
    if (!(recieved_data[0] == 0 && recieved_data[1] == 0 && recieved_data[2] == 0 && recieved_data[3] == 0 && recieved_data[4] == 0)) {            
      a = recieved_data[0];
      b = recieved_data[1];
      c = recieved_data[2];
      d = recieved_data[3];
      e = recieved_data[4];
      Serial.print("f: "); Serial.print(a);
      Serial.print(" b: "); Serial.print(b);
      Serial.print(" l: "); Serial.print(c);
      Serial.print(" r: "); Serial.print(d);
      Serial.print(" f: "); Serial.print(e);
      Serial.println(" ");
    }

    if (c > 0) { // влево 
      analogWrite(LPWM, 0);  
      analogWrite(LPWM2, 0);
      analogWrite(RPWM, c);  
      analogWrite(RPWM2, c);        
    }       
    else if (d > 0) { // вправо
      analogWrite(RPWM, 0);  
      analogWrite(RPWM2, 0);
      analogWrite(LPWM, d);  
      analogWrite(LPWM2, d);
    }       
    else if (a > 0) { // вперед
      analogWrite(LPWM2, 0);
      analogWrite(RPWM, 0); 
      analogWrite(LPWM, a);
      analogWrite(RPWM2, a); 
    } 
    else if (b > 0) { // назад
      analogWrite(LPWM, 0);
      analogWrite(RPWM2, 0); 
      analogWrite(LPWM2, b);
      analogWrite(RPWM, b);        
    }
  } else { //если долго получаем нули, пора остановить двигатели
    i++;
    if (i > 10000) {    
      analogWrite(LPWM, 0);  
      analogWrite(RPWM, 0); 
      analogWrite(LPWM2, 0);  
      analogWrite(RPWM2, 0);        
    }
  }
}
