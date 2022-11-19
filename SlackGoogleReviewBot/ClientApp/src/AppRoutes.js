import { Home } from "./components/Home";
import Tos from './components/Tos';

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: 'privacy',
    element: <Tos />
  },
  // {
  //   path: '/counter',
  //   element: <Counter />
  // },
];

export default AppRoutes;
