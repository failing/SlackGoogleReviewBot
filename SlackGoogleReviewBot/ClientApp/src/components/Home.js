import React from 'react';
import './Home.css'
import { Box } from '@chakra-ui/react';
import { Heading, Image } from '@chakra-ui/react'
import Slider from "react-slick";


const settings = {
  dots: true,
  infinite: true,
  speed: 500,
  slidesToShow: 1,
  slidesToScroll: 1
};

const Home = () => {
  return (

    <Box w='100%' p={8} color='white'>
      <div>
        <Heading textAlign={'center'} pb={8}>Slack Review Bot</Heading>

        <Image
          borderRadius='3xl'
          src='/logo.png'
          alt='Dan Abramov'
        />

        <Heading pt={4} pb={8} size='sm'>Track any Google Business's reviews all within slack!</Heading>

        <Box mt={18} mb={4}>
          <Slider {...settings}>
            <div>
              <Image
                src='/search.png'
                alt='Search'
              />
            </div>
            <div>
              <Image
                src='/search_after.png'
                alt='Search After'
              />
            </div>
            <div>
              <Image
                src='/new_review.png'
                alt='Review'
              />
            </div>
            <div>
              <Image
                src='/blist.png'
                alt='B List'
              />
            </div>
            <div>
              <Image
                src='/blist_results.png'
                alt='B List Results'
              />
            </div>
          </Slider>
        </Box>

        <Box pt={6} display={'flex'} alignItems={'center'} gap={'2'} flexDirection={'row'}>
          <a href="https://slack.com/oauth/v2/authorize?client_id=4328743051921.4322203234226&scope=channels:history,channels:read,chat:write,chat:write.public,commands,groups:history,im:history,incoming-webhook,mpim:history&user_scope="><img alt="Add to Slack" height="40" width="139" src="https://platform.slack-edge.com/img/add_to_slack.png" srcSet="https://platform.slack-edge.com/img/add_to_slack.png 1x, https://platform.slack-edge.com/img/add_to_slack@2x.png 2x" /></a>
          <span>Install it now!</span>
        </Box>

        <Box pt={8}>
          <span >Check the privacy policy <a href="/privacy">here</a></span>
        </Box>

      </div>
    </Box>
  );
}

export { Home };

