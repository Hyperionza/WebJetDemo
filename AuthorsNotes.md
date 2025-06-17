For this assignment I've taken it that AI is an acceptable tool to use - its useful for some things, but its not a solution in its own right. As it turns out it still has too many halucinations but on small contained scopes its ok. Not my normal style of doing things. I believe AI is an extremely useful tool for developers. Emphasis on 'tool', its something that requires human oversight and review (and usually human fixing too), but can speed up some project tasks significantly.

Typically in production workloads, I like to stick to the LTS versions of frameworks, they're more stable, predictable and supported for a longer duration. Meaning there is less need to frequently use developer time to update and potentially deal with breaking changes on mojor version updates. I've used .Net 9 simply because its latest and this is a demo.

I also prefer having different repositories for front-end and api solutions. It was simply convenient for the purposes of the demo to roll it all as one repository.

I stongly believe in separation of concerns hence the use of the Clean Architecture pattern. I've set it up to use Dev Containers becuase I like being able to check out a repository and start work immediately rather than spend a day setting up dependencies (unfortunately very few projects I've had the pleasure of working on ever achieve this, though its becoming easier).

Architecturally, this is a microservice intended to work with other microservices. There is potentially another layer that could be split into its own set of microservices - the actual connections to the 3rd party movie providers. Doing so would leave this a pure aggregator service.

The 3rd party APIs are set up to follow a pull pattern, meaning we initiate queries to retrieve data rather than they notify us via webhooks or other means of a data change. This follows that we should make use of a local caching mechanism. Of the choices of in-memory, redis or database caches, redis makes the most sense as if the api were to scale out the same cache will be used between instances. Redis is also more light-weight than a SQL-database cache. This facilitates short term caching so we dont hit the 3rd parties too frequently as well as shold the 3rd party be unreachable we can still present historical data. I know that movie prices dont change every few minutes so in reality the cache lifespan could be longer, this is just for a sense of capability and thought process. (Note that I'm not quite happy with the caching and I havent tested this aspect as well as I should have, but I think I've spent enough time on the demo).

I had thought about including IaC in the solution to deploy infrastructure to the cloud provider. I realize this may probably be part of a normal microservice project, however, felt it beyond reasonable scope and time allocated for this test.

There is no usage monitoring on the FE, I think setting that up was beyond the scope of the demo

I've used CRA for simplicity and convenience in boilerplating a React site. Its no longer something I'd use in Production as its not being maintained (or at least its not keeping up if it is). So there are some front end dependencies that have vulnerabilities. Because of CRA though, upgrading them to latest is not really a simple thing to do, I had a short crack at it and decided it was a rabbit hole that the demo didnt need exploring.

Regarding security, given the brief, I assumed that this is a public facing site allowing anonymous access to this functionality. Ergo, no authentication or authorisation is taking place.

I've ignored language localisation and locale awareness. (helps that the 3rd party API's only return a price without currency indication).

You'll see some Pokemon exception handling ("Gotta catch 'em all"). Its not always best practice, especially when you need to have different behaviours based off different exception types. But it is useful at entry points to catch unexpected things gracefully.

Having a one liner description of what the demo should encompass does leave a lot open to interpretation and scope creep, and I've let it creep where normally I'd be pushing back to the product owner/ BA to ask if they've considered XYZ. Honestly, I've had a bit of fun going overboard on this one, though there are places where I hope its obvious that I held back, I've mentioned some in this . Using AI as a tool to assist development was not excluded from the demo parameters, though I feel obligated to mention that I used it. I feel its a useful part of the SDLC when used carefully, though any ouput must be human reviewed