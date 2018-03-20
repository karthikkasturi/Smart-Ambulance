void setup() {

  // put your setup code here, to run once:
Serial.begin(9600);
}
void loop() {
  // put your main code here, to run repeatedly:
Serial.println("Medical_IOT_PulseOximeter,John Doe,120,17.3971, 78.4903");
delay(12000);
}
