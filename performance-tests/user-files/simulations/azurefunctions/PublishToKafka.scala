/*
 * Copyright 2011-2019 GatlingCorp (https://gatling.io)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package azurefunctions

import io.gatling.core.Predef._
import io.gatling.http.Predef._
import scala.concurrent.duration._


import org.apache.kafka.clients.producer.ProducerConfig

import com.github.mnogu.gatling.kafka.Predef._

class PublishToKafka extends Simulation {

  val maxRequests = 10000
  val concurrentUsers = 10

  val kafkaConf = kafka
    // Kafka topic name
    .topic("person")
    // Kafka producer configs
    .properties(
      Map(
        ProducerConfig.ACKS_CONFIG -> "1",
        // list of Kafka broker hostname and port pairs
        ProducerConfig.BOOTSTRAP_SERVERS_CONFIG -> "HOST:9092",

        // in most cases, StringSerializer or ByteArraySerializer
        ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG ->
          "org.apache.kafka.common.serialization.StringSerializer",
        ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG ->
          "org.apache.kafka.common.serialization.StringSerializer"))

  val scn = scenario("Kafka Test")
    .repeat(maxRequests, "n") {
      exec(
        kafka("request")
          // message to send
          .send[String]("""{"id": ${n},"email":"jane.doe${n}@company.com","firstName":"Jane${n}","lastName":"Doe${n}"}"""))
    }
  setUp(scn.inject(
        rampUsers(concurrentUsers) during (5 seconds), 
      )
      .protocols(kafkaConf))

}
